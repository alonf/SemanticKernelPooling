﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace SemanticKernelPooling;

/// <summary>
/// Manages kernel pools and provides methods to register and retrieve kernel pools for different AI service providers.
/// Implements both <see cref="IKernelPoolManager"/> and <see cref="IKernelPoolFactoryRegistrar"/>.
/// </summary>
public class KernelPoolManager : IKernelPoolFactoryRegistrar, IKernelPoolManager
{
    private readonly List<AIServiceProviderConfiguration> _configs;

    private readonly Dictionary<AIServiceProviderType, Func<IConfigurationSection, AIServiceProviderConfiguration>>
        _configurationReaders = new();
    private readonly ILogger<KernelPoolManager> _logger;
    private readonly ConcurrentDictionary<AIServiceProviderType,
        Func<AIServiceProviderConfiguration, ILoggerFactory, IKernelPool>> _kernelPoolFactoryMethods = new();
    private readonly ConcurrentDictionary<AIServiceProviderType, IKernelPool> _kernelPools = new();
    private readonly ILoggerFactory _loggerFactory;
    // Dictionary to maintain round-robin indices for each scope
    private readonly ConcurrentDictionary<string, int> _scopeIndices = new();


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

    //the actual method the get or create the kernel pool
    private IKernelPool CreateKernelPool(AIServiceProviderConfiguration config)
    {
        _logger.LogInformation("Creating kernel pool for service provider type {serviceProviderType}", config.ServiceType);

        // Use GetOrAdd to ensure thread-safe, lock-free pool creation
        var kernelPool = _kernelPools.GetOrAdd(config.ServiceType, _ =>
        {
            // This factory method is called only if the pool does not exist
            if (!_kernelPoolFactoryMethods.TryGetValue(config.ServiceType, out var factoryMethod))
            {
                throw new KeyNotFoundException($"No factory method registered for service type {config.ServiceType}");
            }

            var createdPool = factoryMethod(config, _loggerFactory);
            _logger.LogInformation("Kernel pool created for service provider type {serviceProviderType}", config.ServiceType);

            return createdPool;
        });

        return kernelPool;
    }

    //Get a pool by config
    private async Task<KernelWrapper> GetKernelAsync(AIServiceProviderConfiguration config)
    {
        //first check if we already have the required pool
        if (!_kernelPools.TryGetValue(config.ServiceType, out IKernelPool? kernelPool))
        {
            //if not, create the pool
            kernelPool = CreateKernelPool(config);
        }

        //get a kernel from the pool
        var kernelWrapper = await kernelPool.GetKernelAsync();

        return kernelWrapper;
    }

    /// <inheritdoc />
    public async Task<KernelWrapper> GetKernelAsync(AIServiceProviderType aiServiceProviderType)
    {
        //gwt the service type configuration
        var config = _configs.First(c => c.ServiceType == aiServiceProviderType);
        if (config == null)
            throw new InvalidOperationException($"No configuration found for service provider type {aiServiceProviderType}");

        var kernelWrapper = await GetKernelAsync(config);

        return kernelWrapper;
    }

    public async Task<KernelWrapper> GetKernelAsync(string scope)
    {
        // Get all configurations that have the requested scope
        var configs = _configs.Where(c => c.Scopes.Contains(scope)).ToList();

        if (configs.Count == 0)
            throw new InvalidOperationException($"No configuration found for scope {scope}");

        // Use round-robin selection to choose a configuration
        var index = _scopeIndices.AddOrUpdate(scope, 0, (_, oldValue) => (oldValue + 1) % configs.Count);
        var selectedConfig = configs[index];

        var kernelWrapper = await GetKernelAsync(selectedConfig);

        return kernelWrapper;
    }


    private IKernelPoolRegistration<TServiceProviderConfiguration> GetKernelPoolRegistrar<TServiceProviderConfiguration>()
        where TServiceProviderConfiguration : AIServiceProviderConfiguration
    {
        //get the service provider type from the configuration
        var serviceProviderType = _configs.First(c => c.GetType() == typeof(TServiceProviderConfiguration)).ServiceType;

        //get the pool
        if (!_kernelPools.TryGetValue(serviceProviderType, out IKernelPool? kernelPool))
        {
            //if not, create the pool
            kernelPool = CreateKernelPool(serviceProviderType);
        }

        var kernelPoolPreCreationRegistrar = (IKernelPoolRegistration<TServiceProviderConfiguration>)kernelPool;

        return kernelPoolPreCreationRegistrar;
    }

    /// <summary>
    /// Creates a new kernel pool for the specified AI service provider type using the registered factory method.
    /// </summary>
    /// <param name="aiServiceProviderType">The type of the AI service provider.</param>
    /// <returns>An instance of <see cref="IKernelPool"/> created using the factory method.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no factory method is registered for the specified service type.</exception>
    private IKernelPool CreateKernelPool(AIServiceProviderType aiServiceProviderType)
    {
        var config = _configs.FirstOrDefault(c => c.ServiceType == aiServiceProviderType);
        if (config == null)
            throw new InvalidOperationException($"No configuration found for service provider type {aiServiceProviderType}");

        var kernelPool = CreateKernelPool(config);

        return kernelPool;
    }

    /// <inheritdoc />
    public void RegisterForPreKernelCreation<TServiceProviderConfiguration>(Action<IKernelBuilder, TServiceProviderConfiguration, KernelBuilderOptions> action)
        where TServiceProviderConfiguration : AIServiceProviderConfiguration
    {
        var kernelPoolPreCreationRegistrar = GetKernelPoolRegistrar<TServiceProviderConfiguration>();
        
        //register the action
        kernelPoolPreCreationRegistrar.RegisterForPreKernelCreation(action);
    }

    /// <inheritdoc />
    public void RegisterForPreKernelCreation(string scope, Action<IKernelBuilder, AIServiceProviderConfiguration, KernelBuilderOptions, IReadOnlyList<string>> action)
    {
        //go through all the pools configuration, for each, filter by usage key and register the action
        foreach (var config in _configs)
        {
            var configuredScopes = config.Scopes;

            if (configuredScopes.All(s => s != scope))
                continue;

            var serviceProviderType = config.ServiceType;
            if (!_kernelPools.TryGetValue(serviceProviderType, out IKernelPool? kernelPool))
            {
                //if not, create the pool
                kernelPool = CreateKernelPool(serviceProviderType);
            }

            var kernelPoolPreCreationRegistrar = (IKernelPoolRegistration<AIServiceProviderConfiguration>)kernelPool;

            kernelPoolPreCreationRegistrar.RegisterForPreKernelCreation(scope, action);
        }
    }

    /// <inheritdoc />
    public void RegisterForAfterKernelCreation<TServiceProviderConfiguration>(Action<Kernel, TServiceProviderConfiguration> action)
        where TServiceProviderConfiguration : AIServiceProviderConfiguration
    {
        var kernelPoolPreCreationRegistrar = GetKernelPoolRegistrar<TServiceProviderConfiguration>();

        //register the action
        kernelPoolPreCreationRegistrar.RegisterForAfterKernelCreation(action);
    }

    /// <inheritdoc />
    public void RegisterForAfterKernelCreation(string scope, Action<Kernel, AIServiceProviderConfiguration, IReadOnlyList<string>> action)
    {
        //go through all the pools configuration, for each, filter by usage key and register the action
        foreach (var config in _configs)
        {
            var configuredScopes = config.Scopes;

            if (configuredScopes.All(s => s != scope))
                continue;

            var serviceProviderType = config.ServiceType;
            if (!_kernelPools.TryGetValue(serviceProviderType, out IKernelPool? kernelPool))
            {
                //if not, create the pool
                kernelPool = CreateKernelPool(serviceProviderType);
            }

            var kernelPoolPreCreationRegistrar = (IKernelPoolRegistration<AIServiceProviderConfiguration>)kernelPool;

            kernelPoolPreCreationRegistrar.RegisterForAfterKernelCreation(scope, action);
        }
    }

    /// <inheritdoc />
    public void RegisterKernelPoolFactory(AIServiceProviderType azureOpenAI, Func<AIServiceProviderConfiguration, ILoggerFactory, IKernelPool> kernelPoolFactory)
    {
        _logger.LogInformation("Registering kernel pool factory for service provider type {serviceProviderType}", azureOpenAI);

        _kernelPoolFactoryMethods[azureOpenAI] = kernelPoolFactory;
    }

    public void RegisterConfigurationReader(AIServiceProviderType providerType, Func<IConfigurationSection, AIServiceProviderConfiguration> configurationReader)
    {
        _configurationReaders[providerType] = configurationReader;
    }

    public Func<IConfigurationSection, AIServiceProviderConfiguration> GetConfigurationReader(AIServiceProviderType serviceType)
    {
        return _configurationReaders[serviceType];
    }
}