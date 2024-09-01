using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPooling.Connectors.MistralAI
{
    // ReSharper disable once UnusedType.Global

    /// <summary>
    /// Provides extension methods for registering the Mistral AI service provider kernel pool with the service collection.
    /// </summary>
    public static class ServiceExtension
    {
        // ReSharper disable once UnusedMember.Global

        /// <summary>
        /// Registers the Mistral AI service provider kernel pool with the service collection.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the Mistral AI service provider. It enables integration with Mistral's
        /// model APIs using a pooled kernel approach.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        /// <returns>The service collection</returns>
        public static IServiceProvider UseMistralAIKernelPool(this IServiceProvider serviceProvider)
        {
            var registrar = serviceProvider.GetRequiredService<IKernelPoolFactoryRegistrar>();

            // Register the kernel pool factory for Mistral AI
            registrar.RegisterKernelPoolFactory(
                AIServiceProviderType.MistralAI,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new MistralAIKernelPool((MistralAIConfiguration)aiServiceProviderConfiguration, loggerFactory));

            // Register the configuration reader for Mistral AI
            registrar.RegisterConfigurationReader(
                AIServiceProviderType.MistralAI,
                configurationSection => configurationSection.Get<MistralAIConfiguration>()
                                        ?? throw new InvalidOperationException("MistralAI configuration not found."));

            return serviceProvider;
        }
    }
}
