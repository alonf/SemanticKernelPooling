using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SemanticKernelPooling.Connectors.Google
{
    // ReSharper disable once UnusedType.Global

    /// <summary>
    /// Provides extension methods for registering the Google AI service provider kernel pool with the service collection.
    /// </summary>
    public static class ServiceExtension
    {
        // ReSharper disable once UnusedMember.Global

        /// <summary>
        /// Registers the Google AI service provider kernel pool with the service collection.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the Google AI service provider.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        /// <returns>The service collection</returns>
        public static IServiceProvider UseGoogleKernelPool(this IServiceProvider serviceProvider)
        {
            var registrar = serviceProvider.GetRequiredService<IKernelPoolFactoryRegistrar>();

            // Register the kernel pool factory for Google AI
            registrar.RegisterKernelPoolFactory(
                AIServiceProviderType.Google,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new GoogleKernelPool((GoogleConfiguration)aiServiceProviderConfiguration, loggerFactory));

            // Register the configuration reader for Google AI
            registrar.RegisterConfigurationReader(
                AIServiceProviderType.Google,
                configurationSection => configurationSection.Get<GoogleConfiguration>()
                                        ?? throw new InvalidOperationException("Google AI configuration not found."));

            return serviceProvider;
        }
    }
}
