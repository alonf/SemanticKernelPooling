using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;
using SemanticKernelPooling.ServicePools;

namespace SemanticKernelPooling.Connectors.OpenAI;

// ReSharper disable once ClassNeverInstantiated.Global
class AzureOpenAIKernelPool(
    AzureOpenAIConfiguration azureOpenAIConfiguration,
    CustomKernelBuilderConfig customKernelBuilderConfig,
    ILoggerFactory loggerFactory)
    : SpecificAIServicePool<AzureOpenAIConfiguration>(azureOpenAIConfiguration, customKernelBuilderConfig)
{
    protected override void RegisterChatCompletionService(IKernelBuilder kernelBuilder,
        AzureOpenAIConfiguration config)
    {
        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: config.DeploymentName,
            apiKey: config.ApiKey,
            endpoint: config.Endpoint,
            modelId: config.ModelId, // Optional name of the underlying model if the deployment name doesn't match the model name
            serviceId: config.ServiceId, // Optional; for targeting specific services within Semantic Kernel
            httpClient: HttpClient); // Optional; if not provided, the HttpClient from the kernel will be used   
    }

    protected override ILogger Logger { get; } = loggerFactory.CreateLogger<AzureOpenAIKernelPool>();
}