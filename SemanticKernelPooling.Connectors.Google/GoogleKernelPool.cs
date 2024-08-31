using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

namespace SemanticKernelPooling.Connectors.Google;
#pragma warning disable SKEXP0070

/// <summary>
/// Represents a kernel pool specifically designed for the Google AI service provider.
/// </summary>
/// <remarks>
/// This class extends the <see cref="AIServicePool{TServiceProviderConfiguration}"/> class 
/// and provides specific functionality for integrating with the Google AI service provider.
/// It registers the chat completion service with the provided kernel configuration.
/// </remarks>
class GoogleKernelPool(
    GoogleConfiguration googleConfiguration,
    ILoggerFactory loggerFactory)
    : AIServicePool<GoogleConfiguration>(googleConfiguration)
{
    /// <summary>
    /// Registers the chat completion service for the Google AI service provider with the specified kernel builder.
    /// </summary>
    /// <param name="kernelBuilder">The kernel builder to which the chat completion service will be added.</param>
    /// <param name="config">The configuration settings for the Google AI service provider.</param>
    /// <param name="httpClient">
    /// An optional HTTP client instance. If not provided, the default HTTP client from the kernel will be used.
    /// </param>
    protected override void RegisterChatCompletionService(IKernelBuilder kernelBuilder, 
        GoogleConfiguration config,
        HttpClient? httpClient)
    {
        if (!Enum.TryParse(config.ApiVersion, out GoogleAIVersion version))
                version = GoogleAIVersion.V1;

        kernelBuilder.AddGoogleAIGeminiChatCompletion(
            modelId: config.ModelId,
            apiKey: config.ApiKey,
            apiVersion: version, // Optional
            serviceId: string.IsNullOrEmpty(config.ServiceId) ? null : config.ServiceId, // Optional; for targeting specific services within Semantic Kernel
            httpClient: httpClient); // Optional; if not provided, the HttpClient from the kernel will be used
    }

    protected override ILogger Logger { get; } = loggerFactory.CreateLogger<GoogleKernelPool>();
}