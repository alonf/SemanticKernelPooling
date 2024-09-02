using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernelPooling;

/// <summary>
/// Manages kernel pools and provides methods to register and retrieve kernel pools for different AI service providers.
/// Implements both <see cref="IKernelPoolManager"/> and <see cref="IKernelPoolFactoryRegistrar"/>.
/// </summary>
public class KernelPoolManager : IKernelPoolManager
{
    private readonly Lazy<List<AIServiceProviderConfiguration>> _lazyConfigs;
    private readonly IKernelPoolFactoryRegistrar _kernelPoolFactoryRegistrar;
    private readonly ILogger<KernelPoolManager> _logger;
    private readonly ConcurrentDictionary<string, IKernelPool> _kernelPools = new();
    private readonly ILoggerFactory _loggerFactory;
    // Dictionary to maintain round-robin indices for each scope
    private readonly ConcurrentDictionary<string, int> _scopeIndices = new();
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="KernelPoolManager"/> class with the specified configurations and logging facilities.
    /// </summary>
    /// <param name="configuration">The configuration service</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    /// <param name="kernelPoolFactoryRegistrar">The factory that enables kernel pool building</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <exception cref="InvalidOperationException">Thrown if no AI provider service configurations are provided.</exception>
    public KernelPoolManager(
        IConfiguration configuration,
        ILogger<KernelPoolManager> logger,
        IKernelPoolFactoryRegistrar kernelPoolFactoryRegistrar,
        ILoggerFactory loggerFactory)
    {
        // Manually read and bind the configuration section
        _configuration = configuration;
        
        _lazyConfigs = new Lazy<List<AIServiceProviderConfiguration>>(ExtractAndBuildConfigurationList, true);
        
        _kernelPoolFactoryRegistrar = kernelPoolFactoryRegistrar;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    private List<AIServiceProviderConfiguration> AIConfigurations => _lazyConfigs.Value;

    private List<AIServiceProviderConfiguration> ExtractAndBuildConfigurationList()
    {
        var providerConfigs = new List<AIServiceProviderConfiguration>();

        var aiServiceProviders = _configuration.GetSection("AIServiceProviderConfigurations").GetChildren();
        foreach (var aiServiceProvider in aiServiceProviders)
        {
            var serviceProviderType = aiServiceProvider.GetValue<AIServiceProviderType>("ServiceType");
            var scopes = aiServiceProvider.GetSection("Scopes").Get<List<string>>();

            var configurationReader = _kernelPoolFactoryRegistrar.GetConfigurationReader(serviceProviderType);
            var config = configurationReader(aiServiceProvider);

            config.Scopes = scopes ?? [];
            providerConfigs.Add(config);
        }

        return providerConfigs;
    }

    //the actual method the get or create the kernel pool
    private IKernelPool CreateKernelPool(AIServiceProviderConfiguration config)
    {
        _logger.LogInformation("Creating kernel pool for service provider type {serviceProviderType}, name:{uniqueName}", 
            config.ServiceType, config.UniqueName);

        // Use GetOrAdd to ensure thread-safe, lock-free pool creation
        var kernelPool = _kernelPools.GetOrAdd(config.UniqueName, _ =>
        {
            // This factory method is called only if the pool does not exist
            var factoryMethod = _kernelPoolFactoryRegistrar.GetKernelPoolFactory(config.ServiceType);

            var createdPool = factoryMethod(config, _loggerFactory);
            _logger.LogInformation("Kernel pool created for service provider type {serviceProviderType}, name:{uniqueName}", 
                config.ServiceType, config.UniqueName);

            return createdPool;
        });

        return kernelPool;
    }

    //Get a pool by config
    private async Task<KernelWrapper> GetKernelAsync(AIServiceProviderConfiguration config)
    {
        //first check if we already have the required pool
        if (!_kernelPools.TryGetValue(config.UniqueName, out IKernelPool? kernelPool))
        {
            //if not, create the pool
            kernelPool = CreateKernelPool(config);
        }

        //get a kernel from the pool
        var kernelWrapper = await kernelPool.GetKernelAsync();

        return kernelWrapper;
    }

    /// <inheritdoc />
    public async Task<KernelWrapper> GetKernelByNameAsync(string uniqueName)
    {
        //gwt the service type configuration
        var config = AIConfigurations.First(c => c.UniqueName == uniqueName);
        if (config == null)
            throw new InvalidOperationException($"No configuration found for kernel pool name {uniqueName}");

        var kernelWrapper = await GetKernelAsync(config);

        return kernelWrapper;
    }

    public async Task<KernelWrapper> GetKernelByScopeAsync(string scope)
    {
        // Get all configurations that have the requested scope
        var configs = AIConfigurations.Where(c => c.Scopes.Contains(scope)).ToList();

        if (configs.Count == 0)
            throw new InvalidOperationException($"No configuration found for scope {scope}");

        // Use round-robin selection to choose a configuration
        var index = _scopeIndices.AddOrUpdate(scope, 0, (_, oldValue) => (oldValue + 1) % configs.Count);
        var selectedConfig = configs[index];

        var kernelWrapper = await GetKernelAsync(selectedConfig);

        return kernelWrapper;
    }


    //private IKernelPoolRegistration<TServiceProviderConfiguration> GetKernelPoolRegistrar<TServiceProviderConfiguration>(
    //    TServiceProviderConfiguration serviceProviderConfiguration)
    //    where TServiceProviderConfiguration : AIServiceProviderConfiguration
    //{
    //    //get the pool
    //    if (!_kernelPools.TryGetValue(serviceProviderConfiguration.UniqueName, out IKernelPool? kernelPool))
    //    {
    //        //if not, create the pool
    //        kernelPool = CreateKernelPool(serviceProviderConfiguration);
    //    }

    //    var kernelPoolPreCreationRegistrar = (IKernelPoolRegistration<TServiceProviderConfiguration>)kernelPool;

    //    return kernelPoolPreCreationRegistrar;
    //}

    ///// <summary>
    ///// Creates a new kernel pool for the specified AI service provider type using the registered factory method.
    ///// </summary>
    ///// <param name="aiServiceProviderType">The type of the AI service provider.</param>
    ///// <returns>An instance of <see cref="IKernelPool"/> created using the factory method.</returns>
    ///// <exception cref="KeyNotFoundException">Thrown if no factory method is registered for the specified service type.</exception>
    //private IKernelPool CreateKernelPool(AIServiceProviderType aiServiceProviderType)
    //{
    //    var config = AIConfigurations.FirstOrDefault(c => c.ServiceType == aiServiceProviderType);
    //    if (config == null)
    //        throw new InvalidOperationException($"No configuration found for service provider type {aiServiceProviderType}");

    //    var kernelPool = CreateKernelPool(config);

    //    return kernelPool;
    //}

    /// <inheritdoc />
    public void RegisterForPreKernelCreation<TServiceProviderConfiguration>(Action<IKernelBuilder, TServiceProviderConfiguration, KernelBuilderOptions> action)
        where TServiceProviderConfiguration : AIServiceProviderConfiguration
    {
        // Go through all the pools configuration, for each, filter by usage key and register the action
        foreach (var config in AIConfigurations.Where(c => c.GetType() == typeof(TServiceProviderConfiguration)))
        {
            if (_kernelPools.ContainsKey(config.UniqueName))
                continue;

            // If the pool does not exist, create the pool
            var kernelPool = CreateKernelPool(config);

            //register the action
            var kernelPoolPreCreationRegistrar = (IKernelPoolRegistration<AIServiceProviderConfiguration>)kernelPool;
            kernelPoolPreCreationRegistrar.RegisterForPreKernelCreation((Action<IKernelBuilder, AIServiceProviderConfiguration, KernelBuilderOptions>)action);
        }
        
    }

    /// <inheritdoc />
    public void RegisterForPreKernelCreation(string scope, Action<IKernelBuilder, AIServiceProviderConfiguration, KernelBuilderOptions, IReadOnlyList<string>> action)
    {
        // Go through all the pools configuration, for each, filter by usage key and register the action
        foreach (var config in AIConfigurations)
        {
            var configuredScopes = config.Scopes;

            if (configuredScopes.All(s => s != scope))
                continue;

            if (_kernelPools.ContainsKey(config.UniqueName)) 
                continue;

            // If the pool does not exist, create the pool
            var kernelPool = CreateKernelPool(config);
            var kernelPoolPreCreationRegistrar = (IKernelPoolRegistration<AIServiceProviderConfiguration>)kernelPool;
            kernelPoolPreCreationRegistrar.RegisterForPreKernelCreation(scope, action);
        }
    }

    /// <inheritdoc />
    public void RegisterForAfterKernelCreation<TServiceProviderConfiguration>(Action<Kernel, TServiceProviderConfiguration> action)
        where TServiceProviderConfiguration : AIServiceProviderConfiguration
    {
        // Go through all the pools configuration, for each, filter by usage key and register the action
        foreach (var config in AIConfigurations.Where(c => c.GetType() == typeof(TServiceProviderConfiguration)))
        {
            if (_kernelPools.ContainsKey(config.UniqueName))
                continue;

            // If the pool does not exist, create the pool
            var kernelPool = CreateKernelPool(config);
            var kernelPoolPreCreationRegistrar = (IKernelPoolRegistration<AIServiceProviderConfiguration>)kernelPool;

            kernelPoolPreCreationRegistrar.RegisterForAfterKernelCreation((Action<Kernel, AIServiceProviderConfiguration>)action);
        }
    }

    /// <inheritdoc />
    public void RegisterForAfterKernelCreation(string scope, Action<Kernel, AIServiceProviderConfiguration, IReadOnlyList<string>> action)
    {
        //go through all the pools configuration, for each, filter by usage key and register the action
        foreach (var config in AIConfigurations)
        {
            var configuredScopes = config.Scopes;

            if (configuredScopes.All(s => s != scope))
                continue;

            if (!_kernelPools.TryGetValue(config.UniqueName, out IKernelPool? kernelPool))
            {
                //if not, create the pool
                kernelPool = CreateKernelPool(config);
            }

            var kernelPoolPreCreationRegistrar = (IKernelPoolRegistration<AIServiceProviderConfiguration>)kernelPool;

            kernelPoolPreCreationRegistrar.RegisterForAfterKernelCreation(scope, action);
        }
    }
}