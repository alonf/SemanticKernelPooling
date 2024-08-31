using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernelPooling;

/// <summary>
/// Represents an abstract base class for managing a pool of AI service kernels.
/// </summary>
/// <typeparam name="TServiceProviderConfiguration">The type of the service provider configuration.</typeparam>
/// <remarks>
/// This class provides common functionality for managing and pooling kernels for different AI service providers.
/// It supports kernel creation, retrieval, and returning to the pool, as well as pre- and post-initialization steps.
/// </remarks>
public abstract class AIServicePool<TServiceProviderConfiguration> : IKernelPoolRegistration<TServiceProviderConfiguration> where TServiceProviderConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable once ConvertToPrimaryConstructor
    /// <summary>
    /// Initializes a new instance of the <see cref="AIServicePool{TServiceProviderConfiguration}"/> class
    /// with the specified service provider configuration.
    /// </summary>
    /// <param name="aiServiceProviderAIConfiguration">The configuration for the AI service provider.</param>
    protected AIServicePool(TServiceProviderConfiguration aiServiceProviderAIConfiguration)
    {
        AIServiceProviderAIConfiguration = aiServiceProviderAIConfiguration;
        Semaphore = new SemaphoreSlim(1, aiServiceProviderAIConfiguration.InstanceCount);
    }

    private readonly List<Action<IKernelBuilder, TServiceProviderConfiguration, KernelBuilderOptions>> _beforeKernelBuildInitializers = new();
    private readonly List<Action<Kernel, TServiceProviderConfiguration>> _afterKernelBuildInitializers = new();

    private readonly ConcurrentBag<Kernel> _kernels = new();
    private TServiceProviderConfiguration AIServiceProviderAIConfiguration { get; }
    private int CurrentNumberOfKernels { get; } = 0;
    private SemaphoreSlim Semaphore { get; }

    /// <summary>
    /// Registers the chat completion service with the specified kernel builder.
    /// </summary>
    /// <param name="kernelBuilder">The kernel builder to register the service with.</param>
    /// <param name="config">The service provider configuration.</param>
    /// <param name="httpClient">An optional HTTP client to use for service communication.</param>
    protected abstract void RegisterChatCompletionService(IKernelBuilder kernelBuilder,
        TServiceProviderConfiguration config, HttpClient? httpClient);

    /// <summary>
    /// Gets the logger instance used for logging in the specific derived class.
    /// </summary>
    protected abstract ILogger Logger { get; }

    private Kernel CreateKernel()
    {
        var kernelBuilder = Kernel.CreateBuilder();

        TServiceProviderConfiguration newConfig = AIServiceProviderAIConfiguration with { UniqueName = $"{AIServiceProviderAIConfiguration.UniqueName}{CurrentNumberOfKernels}" };
        bool shouldAutoAddChatCompletionService = true;

        KernelBuilderOptions options = new();

        foreach (var preKernelInitializer in _beforeKernelBuildInitializers)
        {
            preKernelInitializer(kernelBuilder, newConfig, options);

            shouldAutoAddChatCompletionService &= options.ShouldAutoAddChatCompletionService;
        }

        if (shouldAutoAddChatCompletionService)
        {
            RegisterChatCompletionService(kernelBuilder, newConfig, options.HttpClient);
        }

        var kernel = kernelBuilder.Build();

        foreach (var postKernelInitializer in _afterKernelBuildInitializers)
        {
            postKernelInitializer(kernel, newConfig);
        }

        return kernel;
    }

    /// <summary>
    /// Asynchronously retrieves a kernel from the pool.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="KernelWrapper"/> 
    /// that provides access to the kernel instance and its associated metadata.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no kernel is available in the pool after waiting for the specified maximum time.
    /// </exception>
    public async Task<KernelWrapper> GetKernelAsync()
    {
        var enterWaitTime = DateTime.Now;
        Logger.LogInformation("Waiting for a kernel to be available in the pool.");

        var result = await Semaphore.WaitAsync(TimeSpan.FromSeconds(AIServiceProviderAIConfiguration.MaxWaitForKernelInSeconds)).ConfigureAwait(false);

        if (!result)
        {
            Logger.LogError("No kernel available in the pool after waiting for {waitTime} ms. Free kernels: {kernelAvailability}",
                (DateTime.Now - enterWaitTime).TotalMilliseconds, _kernels.Count + Semaphore.CurrentCount);
            throw new InvalidOperationException("No available kernels in the pool after waiting.");
        }

        Logger.LogInformation("Kernel available in the pool after waiting for {waitTime} ms. Free kernels: {kernelAvailability}",
            (DateTime.Now - enterWaitTime).TotalMilliseconds, _kernels.Count + Semaphore.CurrentCount);

        KernelWrapper kernelWrapper;

        if (_kernels.TryTake(out var kernel))
        {
            kernelWrapper = new KernelWrapper(kernel, this, Logger);
        }
        else
        {
            var newKernel = CreateKernel();
            Logger.LogInformation("Kernel created. Total created kernels: {totalCreatedKernels}",
                AIServiceProviderAIConfiguration.InstanceCount - Semaphore.CurrentCount);

            kernelWrapper = new KernelWrapper(newKernel, this, Logger);
        }

        return kernelWrapper;
    }

    /// <summary>
    /// Registers an action to be executed before a kernel is created. 
    /// This action allows for customization of the kernel building process.
    /// </summary>
    /// <param name="action">
    /// The action to be executed before kernel creation, which takes an <see cref="IKernelBuilder"/>, 
    /// <typeparamref name="TServiceProviderConfiguration"/>, and <see cref="KernelBuilderOptions"/> as parameters.
    /// </param>
    public void RegisterForPreKernelCreation(Action<IKernelBuilder, TServiceProviderConfiguration, KernelBuilderOptions> action)
    {
        _beforeKernelBuildInitializers.Add(action);
    }

    /// <summary>
    /// Registers an action to be executed after a kernel is created. 
    /// This action allows for additional initialization or configuration of the kernel after it has been built.
    /// </summary>
    /// <param name="action">
    /// The action to be executed after kernel creation, which takes a <see cref="Kernel"/> 
    /// and <typeparamref name="TServiceProviderConfiguration"/> as parameters.
    /// </param>
    public void RegisterForAfterKernelCreation(Action<Kernel, TServiceProviderConfiguration> action)
    {
        _afterKernelBuildInitializers.Add(action);
    }

    /// <summary>
    /// Returns a kernel to the pool, making it available for reuse.
    /// </summary>
    /// <param name="kernel">The kernel to return to the pool.</param>
    public void ReturnKernel(Kernel kernel)
    {
        _kernels.Add(kernel); // Add the kernel back to the pool

        Logger.LogInformation("Kernel returned to the pool. Free kernels: {kernelAvailability}", _kernels.Count + Semaphore.CurrentCount);
        Semaphore.Release();    // Release the semaphore slot
    }
}