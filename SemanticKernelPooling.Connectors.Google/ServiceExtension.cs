using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="services">The service collection to add the Google AI kernel pool to.</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the Google AI service provider.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        public static void UseGoogleKernelPool(this IServiceCollection services)
        {
            services.GetKernelPoolFactoryRegistrar().RegisterKernelPoolFactory(
                AIServiceProviderType.Google,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new GoogleKernelPool((GoogleConfiguration)aiServiceProviderConfiguration, loggerFactory));
        }
    }
}