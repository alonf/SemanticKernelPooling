using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SemanticKernelPooling.Connectors.HuggingFace
{
    // ReSharper disable once UnusedType.Global

    /// <summary>
    /// Provides extension methods for registering the HuggingFace AI service provider kernel pool with the service collection.
    /// </summary>
    public static class ServiceExtension
    {
        // ReSharper disable once UnusedMember.Global

        /// <summary>
        /// Registers the HuggingFace AI service provider kernel pool with the service collection.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the HuggingFace AI service provider. It enables integration with HuggingFace's
        /// model APIs using a pooled kernel approach.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        ///  /// <returns>The service collection</returns>
        public static IServiceProvider UseHuggingFaceKernelPool(this IServiceProvider serviceProvider)
        {
            var registrar = serviceProvider.GetRequiredService<IKernelPoolFactoryRegistrar>();

            // Register the kernel pool factory for HuggingFace
            registrar.RegisterKernelPoolFactory(
                AIServiceProviderType.HuggingFace,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new HuggingFaceKernelPool((HuggingFaceConfiguration)aiServiceProviderConfiguration, loggerFactory));

            // Register the configuration reader for HuggingFace
            registrar.RegisterConfigurationReader(
                AIServiceProviderType.HuggingFace,
                configurationSection => configurationSection.Get<HuggingFaceConfiguration>()
                                        ?? throw new InvalidOperationException("HuggingFace configuration not found."));

            return serviceProvider;
        }
    }
}