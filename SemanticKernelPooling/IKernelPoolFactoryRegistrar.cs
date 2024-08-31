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
    }