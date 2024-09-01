using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPooling;

public class KernelPoolFactoryRegistrar(ILogger<KernelPoolFactoryRegistrar> logger) : IKernelPoolFactoryRegistrar
{
    private readonly ConcurrentDictionary<AIServiceProviderType,
        Func<AIServiceProviderConfiguration, ILoggerFactory, IKernelPool>> _kernelPoolFactoryMethods = new();
    private readonly Dictionary<AIServiceProviderType, Func<IConfigurationSection, AIServiceProviderConfiguration>>
        _configurationReaders = new();

    /// <inheritdoc />
    public void RegisterKernelPoolFactory(AIServiceProviderType azureOpenAI, Func<AIServiceProviderConfiguration, ILoggerFactory, IKernelPool> kernelPoolFactory)
    {
        logger.LogInformation("Registering kernel pool factory for service provider type {serviceProviderType}", azureOpenAI);

        _kernelPoolFactoryMethods[azureOpenAI] = kernelPoolFactory;
    }

    /// <inheritdoc />
    public Func<AIServiceProviderConfiguration, ILoggerFactory, IKernelPool> GetKernelPoolFactory(
        AIServiceProviderType aiServiceProviderType)
    {
        if (!_kernelPoolFactoryMethods.TryGetValue(aiServiceProviderType, out var factory))
        {
            throw new InvalidOperationException($"No kernel pool factory registered for service provider type: {aiServiceProviderType}");
        }

        return factory;
    }

    /// <inheritdoc />
    public void RegisterConfigurationReader(AIServiceProviderType providerType, Func<IConfigurationSection, AIServiceProviderConfiguration> configurationReader)
    {
        _configurationReaders[providerType] = configurationReader;
    }

    /// <inheritdoc />
    public Func<IConfigurationSection, AIServiceProviderConfiguration> GetConfigurationReader(AIServiceProviderType serviceType)
    {
        if (!_configurationReaders.TryGetValue(serviceType, out var reader))
        {
            throw new InvalidOperationException($"No configuration reader registered for service provider type: {serviceType}");
        }

        return reader;
    }
    
}