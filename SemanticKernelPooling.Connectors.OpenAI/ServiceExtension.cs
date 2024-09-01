using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        /// <param name="serviceProvider">The DI service provider</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the Azure OpenAI service provider. It enables integration with Azure OpenAI's
        /// model APIs using a pooled kernel approach.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        /// <returns>The service collection</returns>
        public static IServiceProvider UseAzureOpenAIKernelPool(this IServiceProvider serviceProvider)
        {
            var registrar = serviceProvider.GetRequiredService<IKernelPoolFactoryRegistrar>();

            registrar.RegisterKernelPoolFactory(
                AIServiceProviderType.AzureOpenAI,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new AzureOpenAIKernelPool((AzureOpenAIConfiguration)aiServiceProviderConfiguration,
                        loggerFactory));

            registrar.RegisterConfigurationReader(
                AIServiceProviderType.AzureOpenAI,
                configurationSection => configurationSection.Get<AzureOpenAIConfiguration>()
                                        ?? throw new InvalidOperationException(
                                            "AzureOpenAI configuration not found."));


            return serviceProvider;
        }


        // ReSharper disable once UnusedMember.Global

        /// <summary>
        /// Registers the OpenAI service provider kernel pool with the service collection.
        /// And the configuration reader
        /// </summary>
        /// <param name="serviceProvider">The dependency injection service provider</param>
        /// <remarks>
        /// This method retrieves the <see cref="IKernelPoolFactoryRegistrar"/> from the service collection and uses it to 
        /// register the kernel pool factory for the OpenAI service provider. It enables integration with OpenAI's
        /// model APIs using a pooled kernel approach.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service provider cannot create an instance of <see cref="IKernelPoolManager"/>.
        /// </exception>
        /// <returns>The service collection</returns>
        public static IServiceProvider UseOpenAIKernelPool(this IServiceProvider serviceProvider)
        {
            var registrar = serviceProvider.GetRequiredService<IKernelPoolFactoryRegistrar>();

            registrar.RegisterKernelPoolFactory(
                AIServiceProviderType.OpenAI,
                (aiServiceProviderConfiguration, loggerFactory) =>
                    new OpenAIKernelPool((OpenAIConfiguration)aiServiceProviderConfiguration, loggerFactory)
            );

            registrar.RegisterConfigurationReader(
                AIServiceProviderType.OpenAI,
                configurationSection => configurationSection.Get<OpenAIConfiguration>()
                ?? throw new InvalidOperationException("OpenAI configuration not found.")
            );

            return serviceProvider;
        }
    }
}
