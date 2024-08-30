using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling;

public class KernelPoolManager : IKernelPool
{
    private readonly ConcurrentDictionary<string, IKernelPool> _kernelPools;
    private readonly List<AIServiceProviderConfiguration> _configs;
    private readonly List<Action<Kernel, AIServiceProviderConfiguration>> _afterKernelBuildInitializers = new();
    private readonly ILogger<KernelPoolManager> _logger;
    private readonly int _maxWaitForKernelInSeconds;

    public KernelPoolManager(IOptions<List<AIServiceProviderConfiguration>> options, IConfiguration configuration,
        ILogger<KernelPoolManager> logger)
    {
        _configs = options.Value;
        if (_configs.Count == 0)
            throw new InvalidOperationException("No Azure OpenAI configurations provided.");

        _skKernelPoolSize = configuration.GetValue<int?>("SKKernelPoolSize") ?? throw new ArgumentException("Missing SKKernelPoolSize in configuration");
        _kernelPool = new ConcurrentBag<Kernel>();
        _semaphore = new SemaphoreSlim(_skKernelPoolSize);
        _logger = logger;
        _maxWaitForKernelInSeconds = configuration.GetValue<int?>("SKKernelPoolMaxWait") ?? 60;
    }

    public void Initialize()
    {
        for (int i = 0; i < _skKernelPoolSize; i++)
        {
            var config = _configs[i % _configs.Count];
            var kernel = CreateKernel(config, i);
            _kernelPool.Add(kernel);
        }
    }

    private Kernel CreateKernel(AIServiceProviderConfiguration config, int i)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        
        AIServiceProviderConfiguration newConfig = config with { UniqueName = $"{config.UniqueName}{i}" };
        bool shouldAutoAddChatCompletionService = true;
        HttpClient? httpClient = null;

        foreach (var preKernelInitializer in _beforeKernelBuildInitializers)
        {
            CustomKernelBuilderConfig customConfig = new();
            preKernelInitializer(kernelBuilder, newConfig, customConfig);

            shouldAutoAddChatCompletionService &= customConfig.ShouldAutoAddChatCompletionService;
            httpClient ??= customConfig.HttpClient;
        }

        var kernel = kernelBuilder.Build();
        if (shouldAutoAddChatCompletionService)
        {
            //register chat completion service according to the service type
        }

        foreach (var postKernelInitializer in _afterKernelBuildInitializers)
        {
            postKernelInitializer(kernel, newConfig);
        }

        return kernel;
    }

    public async Task<KernelWrapper> GetKernelAsync()
    {
        var enterWaitTime = DateTime.Now;
        _logger.LogInformation("Waiting for a kernel to be available in the pool.");

        var result = await _semaphore.WaitAsync(TimeSpan.FromSeconds(_maxWaitForKernelInSeconds)).ConfigureAwait(false);
        if (!result)
        {
            _logger.LogError("No kernel available in the pool after waiting for {waitTime} ms. Free kernels: {kernelAvailability}",
                (DateTime.Now - enterWaitTime).TotalMilliseconds, _kernelPool.Count);
            throw new InvalidOperationException("No available kernels in the pool after waiting.");
        }

        _logger.LogInformation("Kernel available in the pool after waiting for {waitTime} ms. Free kernels: {kernelAvailability}",
            (DateTime.Now - enterWaitTime).TotalMilliseconds, _kernelPool.Count);

        if (_kernelPool.TryTake(out var kernel))
        {
            return new KernelWrapper(kernel, this, _logger);
        }

        // In case something went wrong, and no kernel was available after waiting
        throw new InvalidOperationException("No available kernels in the pool after waiting.");
    }

    public void RegisterForPreKernelCreation<TServiceProviderConfiguration>(Action<IKernelBuilder, TServiceProviderConfiguration, CustomKernelBuilderConfig> action) 
        where TServiceProviderConfiguration : AIServiceProviderConfiguration
    {
        if (!_kernelPool.IsEmpty)
            throw new InvalidOperationException("Cannot register for kernel creation when there are kernels in the pool.");

        _beforeKernelBuildInitializers.Add(action);
    }

    public void RegisterForAfterKernelCreation<TServiceProviderConfiguration>(Action<IKernelBuilder, TServiceProviderConfiguration> action)
        where TServiceProviderConfiguration : AIServiceProviderConfiguration
    {
        if (!_kernelPool.IsEmpty)
            throw new InvalidOperationException("Cannot register for kernel creation when there are kernels in the pool.");

        _afterKernelBuildInitializers.Add(action);
    }


    public void ReturnKernel(Kernel kernel)
    {
        _kernelPool.Add(kernel); // Add the kernel back to the pool

        _logger.LogInformation("Kernel returned to the pool. Free kernels: {kernelAvailability}", _kernelPool.Count);
        _semaphore.Release();    // Release the semaphore slot
    }
}