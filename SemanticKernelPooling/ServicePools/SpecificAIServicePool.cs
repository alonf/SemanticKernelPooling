using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.ServicePools;

public abstract class SpecificAIServicePool<TServiceProviderConfiguration> : IKernelPoolRegistration<TServiceProviderConfiguration> where TServiceProviderConfiguration : AIServiceProviderConfiguration
{
    public SpecificAIServicePool(TServiceProviderConfiguration aiServiceProviderConfiguration,
        CustomKernelBuilderConfig customKernelBuilderConfig)
    {
        HttpClient = customKernelBuilderConfig.HttpClient;
        ShouldAutoAddChatCompletionService = customKernelBuilderConfig.ShouldAutoAddChatCompletionService;
        AIServiceProviderConfiguration = aiServiceProviderConfiguration;
        _semaphore = new SemaphoreSlim(1, aiServiceProviderConfiguration.InstanceCount);
    }
    public HttpClient? HttpClient { get; }
    public bool ShouldAutoAddChatCompletionService { get; }

    private readonly List<Action<IKernelBuilder, TServiceProviderConfiguration, CustomKernelBuilderConfig>> _beforeKernelBuildInitializers = new();
    private readonly List<Action<Kernel, TServiceProviderConfiguration>> _afterKernelBuildInitializers = new();

    private ConcurrentBag<Kernel> _kernels = new();
    private TServiceProviderConfiguration AIServiceProviderConfiguration { get; }
    private int CurrentNumberOfKernels { get; set; } = 0;
    private SemaphoreSlim _semaphore { get; init; }

    protected abstract void RegisterChatCompletionService(IKernelBuilder kernelBuilder,
        TServiceProviderConfiguration config);


    protected abstract ILogger Logger { get; }

    private async Task<Kernel> CreateKernelAsync()
    {
        var kernelBuilder = Kernel.CreateBuilder();

        TServiceProviderConfiguration newConfig = AIServiceProviderConfiguration with { UniqueName = $"{AIServiceProviderConfiguration.UniqueName}{CurrentNumberOfKernels}" };
        bool shouldAutoAddChatCompletionService = true;

        foreach (var preKernelInitializer in _beforeKernelBuildInitializers)
        {
            CustomKernelBuilderConfig customConfig = new();
            preKernelInitializer(kernelBuilder, newConfig, customConfig);

            shouldAutoAddChatCompletionService &= customConfig.ShouldAutoAddChatCompletionService;
        }

        if (shouldAutoAddChatCompletionService)
        {
            await RegisterChatCompletionServiceAsync(kernelBuilder, newConfig);
        }

        var kernel = kernelBuilder.Build();

        foreach (var postKernelInitializer in _afterKernelBuildInitializers)
        {
            postKernelInitializer(kernel, newConfig);
        }

        return kernel;
    }

    public async Task<KernelWrapper> GetKernelAsync()
    {
        var enterWaitTime = DateTime.Now;
        Logger.LogInformation("Waiting for a kernel to be available in the pool.");

        var result = await _semaphore.WaitAsync(TimeSpan.FromSeconds(AIServiceProviderConfiguration.MaxWaitForKernelInSeconds)).ConfigureAwait(false);

        if (!result)
        {
            Logger.LogError("No kernel available in the pool after waiting for {waitTime} ms. Free kernels: {kernelAvailability}",
                (DateTime.Now - enterWaitTime).TotalMilliseconds, _kernels.Count + _semaphore.CurrentCount);
            throw new InvalidOperationException("No available kernels in the pool after waiting.");
        }

        Logger.LogInformation("Kernel available in the pool after waiting for {waitTime} ms. Free kernels: {kernelAvailability}",
            (DateTime.Now - enterWaitTime).TotalMilliseconds, _kernels.Count + _semaphore.CurrentCount);

        KernelWrapper kernelWrapper;

        if (_kernels.TryTake(out var kernel))
        {
            kernelWrapper = new KernelWrapper(kernel, this, Logger);
        }
        else
        {
            var newKernel = await CreateKernelAsync();
            Logger.LogInformation("Kernel created. Total created kernels: {totalCreatedKernels}",
                AIServiceProviderConfiguration.InstanceCount - _semaphore.CurrentCount);

            kernelWrapper = new KernelWrapper(newKernel, this, Logger);
        }

        return kernelWrapper;
    }

    public void RegisterForPreKernelCreation(Action<IKernelBuilder, TServiceProviderConfiguration, CustomKernelBuilderConfig> action)
    {
        _beforeKernelBuildInitializers.Add(action);
    }

    public void RegisterForAfterKernelCreation(Action<Kernel, TServiceProviderConfiguration> action)
    {
        _afterKernelBuildInitializers.Add(action);
    }

    public void ReturnKernel(Kernel kernel)
    {
        _kernels.Add(kernel); // Add the kernel back to the pool

        Logger.LogInformation("Kernel returned to the pool. Free kernels: {kernelAvailability}", _kernels.Count + _semaphore.CurrentCount);
        _semaphore.Release();    // Release the semaphore slot
    }
}