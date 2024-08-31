using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernelPooling.Connectors.MistralAI;

#pragma warning disable SKEXP0070

/// <summary>
/// Represents a kernel pool specifically designed for the Mistral AI service provider.
/// </summary>
/// <remarks>
/// This class extends the <see cref="AIServicePool{TServiceProviderConfiguration}"/> class 
/// and provides specific functionality for integrating with the Mistral AI service provider.
/// It registers the chat completion service with the provided kernel configuration.
/// </remarks>
class MistralAIKernelPool(
        MistralAIConfiguration mistralAIConfiguration,
        ILoggerFactory loggerFactory)
        : AIServicePool<MistralAIConfiguration>(mistralAIConfiguration)
{
    /// <summary>
    /// Registers the chat completion service for the Mistral AI service provider with the specified kernel builder.
    /// </summary>
    /// <param name="kernelBuilder">The kernel builder to which the chat completion service will be added.</param>
    /// <param name="config">The configuration settings for the Mistral AI service provider.</param>
    /// <param name="httpClient">
    /// An optional HTTP client instance. If not provided, the default HTTP client from the kernel will be used.
    /// </param>
    protected override void RegisterChatCompletionService(IKernelBuilder kernelBuilder, MistralAIConfiguration config,
        HttpClient? httpClient)
    {
        kernelBuilder.AddMistralChatCompletion(
            modelId: config.ModelId,
            apiKey: config.ApiKey,
            endpoint: string.IsNullOrEmpty(config.Endpoint) ? null : new Uri(config.Endpoint), // Optional
            serviceId: string.IsNullOrEmpty(config.ServiceId) ? null : config.ServiceId, // Optional; for targeting specific services within Semantic Kernel
            httpClient: httpClient); // Optional; if not provided, the HttpClient from the kernel will be used
    }

    /// <summary>
    /// Gets the logger instance used for logging in this class.
    /// </summary>
    protected override ILogger Logger { get; } = loggerFactory.CreateLogger<MistralAIKernelPool>();
}

#pragma warning restore SKEXP0070
