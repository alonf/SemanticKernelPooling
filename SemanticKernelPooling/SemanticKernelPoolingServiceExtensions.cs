using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernelPooling;

// ReSharper disable once UnusedType.Global
public static class SemanticKernelPoolingServiceExtensions
{
    /// <summary>
    /// Registers the Semantic Kernel Pooling services and all configured AI service providers 
    /// with a specified lifetime (Singleton, Scoped, or Transient). Singleton by default
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="lifetime">
    /// Specifies the service lifetime for the Semantic Kernel Pooling services.
    /// </param>
    /// <returns>The updated service collection.</returns>
    /// <example>
    /// This example demonstrates how to use the method to register services with different lifetimes:
    /// <code>
    /// services.UseSemanticKernelPooling(ServiceLifetime.Singleton);   // Register as Singleton
    /// services.UseSemanticKernelPooling(ServiceLifetime.Scoped);      // Register as Scoped
    /// services.UseSemanticKernelPooling(ServiceLifetime.Transient);   // Register as Transient
    /// </code>
    /// </example>
    public static IServiceCollection UseSemanticKernelPooling(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        services.Add(new(typeof(IKernelPoolManager), typeof(KernelPoolManager), lifetime));
        services.AddSingleton<IKernelPoolFactoryRegistrar, KernelPoolFactoryRegistrar>();

        return services;
    }
}