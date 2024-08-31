using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0070

namespace SemanticKernelPooling.Connectors.HuggingFace;

/// <summary>
/// Represents a kernel pool specifically designed for the HuggingFace AI service provider.
/// </summary>
/// <remarks>
/// This class extends the <see cref="AIServicePool{TServiceProviderConfiguration}"/> class 
/// and provides specific functionality for integrating with the HuggingFace AI service provider.
/// It registers the chat completion service with the provided kernel configuration.
/// </remarks>
class HuggingFaceKernelPool(
    HuggingFaceConfiguration huggingFaceConfiguration,
    ILoggerFactory loggerFactory)
    : AIServicePool<HuggingFaceConfiguration>(huggingFaceConfiguration)
{
    /// <summary>
    /// Registers the chat completion service for the HuggingFace AI service provider with the specified kernel builder.
    /// </summary>
    /// <param name="kernelBuilder">The kernel builder to which the chat completion service will be added.</param>
    /// <param name="config">The configuration settings for the HuggingFace AI service provider.</param>
    /// <param name="httpClient">
    /// An optional HTTP client instance. If not provided, the default HTTP client from the kernel will be used.
    /// </param>
    protected override void RegisterChatCompletionService(
        IKernelBuilder kernelBuilder,
        HuggingFaceConfiguration config,
        HttpClient? httpClient)
    {
        kernelBuilder.AddHuggingFaceChatCompletion(
            model: config.Model,
            apiKey: config.ApiKey,
            endpoint: new Uri(config.Endpoint),
            serviceId: string.IsNullOrEmpty(config.ServiceId) ? null : config.ServiceId, // Optional; for targeting specific services within Semantic Kernel
            httpClient: httpClient); // Optional; if not provided, the HttpClient from the kernel will be used   
    }

    /// <summary>
    /// Gets the logger instance used for logging in this class.
    /// </summary>
    protected override ILogger Logger { get; } = loggerFactory.CreateLogger<HuggingFaceKernelPool>();
}

#pragma warning restore SKEXP0070
