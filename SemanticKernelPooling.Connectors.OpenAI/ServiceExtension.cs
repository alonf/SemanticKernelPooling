using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernelPooling.Connectors.OpenAI
{
    // ReSharper disable once UnusedType.Global

    /// <summary>
    /// Provides extension methods for registering OpenAI and Azure OpenAI service provider kernel pools with the service collection.
    /// </summary>
    public static class ServiceExtension
    {
        // ReSharper disable once UnusedMember.Global

        /// <summary>
        /// Registers the Azure OpenAI service provider kernel pool with the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the Azure OpenAI kernel pool to.</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the Azure OpenAI service provider. It enables integration with Azure OpenAI's
        /// model APIs using a pooled kernel approach.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        public static void UseAzureOpenAIKernelPool(this IServiceCollection services)
        {
            services.GetKernelPoolFactoryRegistrar().RegisterKernelPoolFactory(
                AIServiceProviderType.AzureOpenAI,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new AzureOpenAIKernelPool((AzureOpenAIConfiguration)aiServiceProviderConfiguration, loggerFactory));
        }

        // ReSharper disable once UnusedMember.Global

        /// <summary>
        /// Registers the OpenAI service provider kernel pool with the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the OpenAI kernel pool to.</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the OpenAI service provider. It enables integration with OpenAI's
        /// model APIs using a pooled kernel approach.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        public static void UseOpenAIKernelPool(this IServiceCollection services)
        {
            services.GetKernelPoolFactoryRegistrar().RegisterKernelPoolFactory(
                AIServiceProviderType.OpenAI,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new OpenAIKernelPool((OpenAIConfiguration)aiServiceProviderConfiguration, loggerFactory));
        }
    }
}
