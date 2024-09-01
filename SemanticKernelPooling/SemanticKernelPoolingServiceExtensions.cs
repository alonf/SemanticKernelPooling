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
        // Register the KernelPoolManager as a singleton
        services.AddSingleton<IKernelPoolManager, KernelPoolManager>();
        services.AddSingleton<IKernelPoolFactoryRegistrar, KernelPoolFactoryRegistrar>();

        return services;
    }
}