using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="services">The service collection to add the HuggingFace AI kernel pool to.</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the HuggingFace AI service provider. It enables integration with HuggingFace's
        /// model APIs using a pooled kernel approach.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        public static void UseHuggingFaceKernelPool(this ServiceCollection services)
        {
            services.GetKernelPoolFactoryRegistrar().RegisterKernelPoolFactory(
                AIServiceProviderType.HuggingFace,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new HuggingFaceKernelPool((HuggingFaceConfiguration)aiServiceProviderConfiguration, loggerFactory));
        }
    }
}