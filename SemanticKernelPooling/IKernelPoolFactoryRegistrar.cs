using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPooling;
    /// <summary>
    /// Interface for registering kernel pool factories for different AI service providers.
    /// </summary>
    public interface IKernelPoolFactoryRegistrar
    {
        /// <summary>
        /// Registers a factory method for creating a kernel pool for a specific AI service provider type.
        /// </summary>
        /// <param name="aiServiceProviderType">The type of the AI service provider (e.g., AzureOpenAI, OpenAI, MistralAI, etc.).</param>
        /// <param name="kernelPoolFactory">
        /// A factory method that creates an instance of <see cref="IKernelPool"/> for the specified 
        /// AI service provider configuration and <see cref="ILoggerFactory"/>.
        /// </param>
        void RegisterKernelPoolFactory(
            AIServiceProviderType aiServiceProviderType,
            Func<AIServiceProviderConfiguration, ILoggerFactory, IKernelPool> kernelPoolFactory);


        /// <summary>
        /// Registers a configuration reader function for a specific AI service provider type.
        /// </summary>
        /// <param name="providerType">The type of the AI service provider (e.g., AzureOpenAI, OpenAI, MistralAI, etc.).</param>
        /// <param name="configurationReader">
        /// A function that reads a configuration section and returns an instance of <see cref="AIServiceProviderConfiguration"/>.
        /// </param>
        /// <remarks>
        /// This method registers a function that knows how to read and parse the configuration section specific to 
        /// an AI service provider type. It allows dynamic deserialization of provider configurations based on the 
        /// provider type defined in the application configuration.
        /// </remarks>
        void RegisterConfigurationReader(
            AIServiceProviderType providerType,
            Func<IConfigurationSection, AIServiceProviderConfiguration> configurationReader);

        /// <summary>
        /// Get a method that knows how to read the specific configuration for a given AI service provider type.
        /// </summary>
        /// <param name="serviceType">The specific service</param>
        /// <returns></returns>
        Func<IConfigurationSection, AIServiceProviderConfiguration> GetConfigurationReader(AIServiceProviderType serviceType);
    }