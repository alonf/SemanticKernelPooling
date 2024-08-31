using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernelPooling.Connectors.OpenAI;

/// <summary>
/// Represents a kernel pool specifically designed for the Azure OpenAI service provider.
/// </summary>
/// <remarks>
/// This class extends the <see cref="AIServicePool{TServiceProviderConfiguration}"/> class 
/// and provides specific functionality for integrating with the Azure OpenAI service provider.
/// It registers the chat completion service with the provided kernel configuration.
/// </remarks>
class AzureOpenAIKernelPool(
    AzureOpenAIConfiguration azureOpenAIConfiguration,
    ILoggerFactory loggerFactory)
    : AIServicePool<AzureOpenAIConfiguration>(azureOpenAIConfiguration)
{
    /// <summary>
    /// Registers the chat completion service for the Azure OpenAI service provider with the specified kernel builder.
    /// </summary>
    /// <param name="kernelBuilder">The kernel builder to which the chat completion service will be added.</param>
    /// <param name="config">The configuration settings for the Azure OpenAI service provider.</param>
    /// <param name="httpClient">
    /// An optional HTTP client instance. If not provided, the default HTTP client from the kernel will be used.
    /// </param>
    protected override void RegisterChatCompletionService(IKernelBuilder kernelBuilder,
        AzureOpenAIConfiguration config, HttpClient? httpClient)
    {
        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: config.DeploymentName,
            apiKey: config.ApiKey,
            endpoint: config.Endpoint,
            modelId: string.IsNullOrEmpty(config.ModelId) ? null : config.ModelId, // Optional name of the underlying model if the deployment name doesn't match the model name
            serviceId: string.IsNullOrEmpty(config.ServiceId) ? null : config.ServiceId, // Optional; for targeting specific services within Semantic Kernel
            httpClient: httpClient); // Optional; if not provided, the HttpClient from the kernel will be used   
    }

    /// <summary>
    /// Gets the logger instance used for logging in this class.
    /// </summary>
    protected override ILogger Logger { get; } = loggerFactory.CreateLogger<AzureOpenAIKernelPool>();
}
