using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernelPooling.Connectors.Other;

#pragma warning disable SKEXP0010

/// <summary>
/// Represents a kernel pool specifically designed for the Other AI service provider.
/// </summary>
/// <remarks>
/// This class extends the <see cref="AIServicePool{TServiceProviderConfiguration}"/> class 
/// and provides specific functionality for integrating with the Other AI service provider.
/// It registers the chat completion service with the provided kernel configuration.
/// </remarks>
/// <summary>
/// Initializes a new instance of the <see cref="OtherAIKernelPool"/> class with the specified configuration and logger factory.
/// </summary>
/// <param name="otherAIConfiguration">The configuration settings for the Other AI service provider.</param>
/// <param name="loggerFactory">The factory used to create logger instances.</param>
class OtherAIKernelPool(
    OtherAIConfiguration otherAIConfiguration,
    ILoggerFactory loggerFactory)
    : AIServicePool<OtherAIConfiguration>(otherAIConfiguration)
{
    /// <summary>
    /// Registers the chat completion service for Other AI.
    /// </summary>
    /// <param name="kernelBuilder">The kernel builder.</param>
    /// <param name="config">The Other AI configuration.</param>
    /// <param name="httpClient">The HTTP client.</param>
    protected override void RegisterChatCompletionService(IKernelBuilder kernelBuilder, OtherAIConfiguration config,
        HttpClient? httpClient)
    {
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: config.ModelId,
            apiKey: config.ApiKey,
            endpoint: new Uri(config.Endpoint),
            serviceId: string.IsNullOrEmpty(config.ServiceId) ? null : config.ServiceId, // Optional; for targeting specific services within Semantic Kernel
            httpClient: httpClient); // Optional; if not provided, the HttpClient from the kernel will be used
    }

    /// <summary>
    /// Gets the logger for Other AI kernel pool.
    /// </summary>
    protected override ILogger Logger { get; } = loggerFactory.CreateLogger<OtherAIKernelPool>();
}

#pragma warning restore SKEXP0010