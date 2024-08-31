using Microsoft.Extensions.DependencyInjection;

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
    /// <param name="services">The service collection to add the Other AI kernel pool to.</param>
    public static void UseOtherAIKernelPool(this IServiceCollection services)
    {
        services.GetKernelPoolFactoryRegistrar().RegisterKernelPoolFactory(AIServiceProviderType.OtherAI,
            (aiServiceProviderConfiguration, loggerFactory) =>
                new OtherAIKernelPool((OtherAIConfiguration)aiServiceProviderConfiguration, loggerFactory));
    }
}