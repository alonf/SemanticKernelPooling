using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SemanticKernelPooling;

/// <summary>
/// Manages kernel pools and provides methods to register and retrieve kernel pools for different AI service providers.
/// Implements both <see cref="IKernelPoolManager"/> and <see cref="IKernelPoolFactoryRegistrar"/>.
/// </summary>
public class KernelPoolManager : IKernelPoolFactoryRegistrar, IKernelPoolManager
{
    private readonly List<AIServiceProviderConfiguration> _configs;
    private readonly ILogger<KernelPoolManager> _logger;
    private readonly ConcurrentDictionary<AIServiceProviderType,
        Func<AIServiceProviderConfiguration, ILoggerFactory, IKernelPool>> _kernelPoolFactoryMethods = new();
    private readonly ConcurrentDictionary<AIServiceProviderType, IKernelPool> _kernelPools = new();
    private readonly ILoggerFactory _loggerFactory;


    /// <summary>
    /// Initializes a new instance of the <see cref="KernelPoolManager"/> class with the specified configurations and logging facilities.
    /// </summary>
    /// <param name="options">The options containing the list of AI service provider configurations.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <exception cref="InvalidOperationException">Thrown if no AI provider service configurations are provided.</exception>
    public KernelPoolManager(IOptions<List<AIServiceProviderConfiguration>> options, 
        ILogger<KernelPoolManager> logger,
        ILoggerFactory loggerFactory)
    {
        _configs = options.Value;
        if (_configs.Count == 0)
            throw new InvalidOperationException("No AI provider service configurations provided.");

        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public async Task<KernelWrapper> GetKernelAsync(AIServiceProviderType aiServiceProviderType)
    {
        //get the configuration for the service provider type
        var config = _configs.FirstOrDefault(c => c.ServiceType == aiServiceProviderType);
        
        if (config == null)
            throw new InvalidOperationException($"No configuration found for service provider type {aiServiceProviderType}");

        //first check if we already have the required pool
        if (!_kernelPools.TryGetValue(aiServiceProviderType, out IKernelPool? kernelPool))
        {
            //if not, create the pool
            kernelPool = CreateKernelPool(aiServiceProviderType);
        }

        //get a kernel from the pool
        var kernelWrapper = await kernelPool.GetKernelAsync();

        return kernelWrapper;
    }

    /// <summary>
    /// Creates a new kernel pool for the specified AI service provider type using the registered factory method.
    /// </summary>
    /// <param name="aiServiceProviderType">The type of the AI service provider.</param>
    /// <returns>An instance of <see cref="IKernelPool"/> created using the factory method.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no factory method is registered for the specified service type.</exception>

    private IKernelPool CreateKernelPool(AIServiceProviderType aiServiceProviderType)
    {
        _logger.LogInformation("Creating kernel pool for service provider type {serviceProviderType}", aiServiceProviderType);

        //get the factory method, throw exception if not found
        if (!_kernelPoolFactoryMethods.TryGetValue(aiServiceProviderType, out var factoryMethod))
            throw new KeyNotFoundException($"No factory method registered for service type {aiServiceProviderType}");

        //create the pool
        var kernelPool = factoryMethod(_configs.First(c => c.ServiceType == aiServiceProviderType), _loggerFactory);

        //store the pool. only if it is not exist. Be aware that concurrent call may try to store it
        if (!_kernelPools.TryAdd(aiServiceProviderType, kernelPool))
        {
            kernelPool = _kernelPools[aiServiceProviderType]; //take the existing one
        }
        
        _logger.LogInformation("Kernel pool created for service provider type {serviceProviderType}", aiServiceProviderType);

        //service pools may be created concurrently, ensure we get the correct pool
        return kernelPool;
    }

    /// <inheritdoc />
    public void RegisterKernelPoolFactory(AIServiceProviderType azureOpenAI, Func<AIServiceProviderConfiguration, ILoggerFactory, IKernelPool> kernelPoolFactory)
    {
        _logger.LogInformation("Registering kernel pool factory for service provider type {serviceProviderType}", azureOpenAI);

        _kernelPoolFactoryMethods[azureOpenAI] = kernelPoolFactory;
    }
}