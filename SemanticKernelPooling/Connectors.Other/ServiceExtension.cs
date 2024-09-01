using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SemanticKernelPooling.Connectors.Other;

/// <summary>
/// Provides extension methods for registering specific AI service provider kernel pools with the service collection.
/// </summary>
public static class ServiceExtension
{
    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Registers the Other AI service provider kernel pool with the service collection.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider</param>
    /// <returns>The service collection</returns>
    public static IServiceProvider UseOtherAIKernelPool(this IServiceProvider serviceProvider)
    {
        var registrar = serviceProvider.GetRequiredService<IKernelPoolFactoryRegistrar>();

        // Register the kernel pool factory for Other AI
        registrar.RegisterKernelPoolFactory(
            AIServiceProviderType.OtherAI, // Adjust the enum or type to match your actual AI provider type
            (aiServiceProviderConfiguration, loggerFactory) =>
                new OtherAIKernelPool((OtherAIConfiguration)aiServiceProviderConfiguration, loggerFactory));

        // Register the configuration reader for Other AI
        registrar.RegisterConfigurationReader(
            AIServiceProviderType.OtherAI, // Adjust the enum or type to match your actual AI provider type
            configurationSection => configurationSection.Get<OtherAIConfiguration>()
                                    ?? throw new InvalidOperationException("Other AI configuration not found."));
        
        return serviceProvider;
    }
}