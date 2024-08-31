using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPooling;

// ReSharper disable once UnusedType.Global
public static class SemanticKernelPoolingServiceExtensions
{
    /// <summary>
    /// Registers the Semantic Kernel Pooling services and all configured AI service providers.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated service collection.</returns>
    // ReSharper disable once UnusedMember.Global
    public static IServiceCollection UseSemanticKernelPooling(this IServiceCollection services)
    {
        // Build a service provider to access registered services
        using var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetService<IConfiguration>();

        if (configuration == null)
        {
            throw new InvalidOperationException("SemanticKernelPooling: No configuration provided.");
        }

        var logger = serviceProvider.GetService<ILogger<KernelPoolManager>>();
        var registrar = serviceProvider.GetService<IKernelPoolFactoryRegistrar>();

        if (registrar == null)
        {
            throw new InvalidOperationException("SemanticKernelPooling: KernelPoolFactoryRegistrar is not registered.");
        }

        // Use the custom deserialization method to handle different derived classes
        var providerConfigs = GetProviderConfigurations(configuration, registrar, logger);

        if (providerConfigs == null || providerConfigs.Count == 0)
        {
            logger?.LogError("SemanticKernelPooling: No service provider configurations provided.");
            throw new InvalidOperationException("SemanticKernelPooling: No service provider configurations provided.");
        }

        services.AddSingleton(providerConfigs);
        services.AddSingleton<IKernelPoolManager, KernelPoolManager>();

        return services;
    }

    /// <summary>
    /// Custom deserialization method to retrieve provider configurations based on ServiceType.
    /// </summary>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="registrar">The kernel pool factory registrar used to get configuration readers.</param>
    /// <param name="logger">The logger instance for error handling and logging.</param>
    /// <returns>A list of AI service provider configurations.</returns>
    private static List<AIServiceProviderConfiguration> GetProviderConfigurations(
        IConfiguration configuration,
        IKernelPoolFactoryRegistrar registrar,
        ILogger? logger)
    {
        var providerConfigsSection = configuration.GetSection("ServiceProviderConfigurations");

        List<AIServiceProviderConfiguration> providerConfigs = new();

        foreach (var section in providerConfigsSection.GetChildren())
        {
            var serviceTypeString = section.GetValue<string>("ServiceType");

            if (Enum.TryParse(serviceTypeString, out AIServiceProviderType serviceType))
            {
                var configReader = registrar.GetConfigurationReader(serviceType);
                if (configReader == null)
                {
                    logger?.LogError($"No configuration reader registered for provider type: {serviceType}");
                    throw new InvalidOperationException($"No configuration reader registered for provider type: {serviceType}");
                }

                try
                {
                    var config = configReader(section);
                    providerConfigs.Add(config);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, $"Error deserializing configuration for provider type {serviceType}");
                }
            }
            else
            {
                logger?.LogError($"Unsupported provider type: {serviceTypeString}");
            }
        }

        return providerConfigs;
    }

    /// <summary>
    /// Retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection.
    /// </summary>
    /// <param name="services">The service collection to retrieve the factory registrar from.</param>
    /// <returns>An instance of <see cref="IKernelPoolFactoryRegistrar"/> used to register kernel pool factories.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
    /// </exception>
    public static IKernelPoolFactoryRegistrar GetKernelPoolFactoryRegistrar(this IServiceCollection services)
    {
        var kernelPoolManager =
            services.BuildServiceProvider().GetRequiredService<IKernelPoolManager>();

        var pollManagerRegistrar = (IKernelPoolFactoryRegistrar)kernelPoolManager;

        return pollManagerRegistrar;
    }

}