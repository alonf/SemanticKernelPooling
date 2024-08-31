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
        var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

        if (configuration == null)
        {
            throw new InvalidOperationException("SemanticKernelPooling: No configuration provided.");
        }

        var providerConfigs = configuration.GetSection("ServiceProviderConfigurations").Get<List<AIServiceProviderConfiguration>>();
        var logger = services.BuildServiceProvider().GetService<ILogger<KernelPoolManager>>();

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
    /// Retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection.
    /// </summary>
    /// <param name="services">The service collection to retrieve the factory registrar from.</param>
    /// <returns>An instance of <see cref="IKernelPoolFactoryRegistrar"/> used to register kernel pool factories.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
    /// </exception>
    public static IKernelPoolFactoryRegistrar GetKernelPoolFactoryRegistrar(this ServiceCollection services)
    {
        var kernelPoolManager =
            services.BuildServiceProvider().GetRequiredService<IKernelPoolManager>();

        var pollManagerRegistrar = (IKernelPoolFactoryRegistrar)kernelPoolManager;

        return pollManagerRegistrar;
    }

}