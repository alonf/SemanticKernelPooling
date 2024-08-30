using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;
using SemanticKernelPooling.ServicePools;

namespace SemanticKernelPooling.Connectors.OpenAI;

// ReSharper disable once UnusedType.Global
class OpenAIKernelPool(
    OpenAIConfiguration openAIConfiguration,
    CustomKernelBuilderConfig customKernelBuilderConfig,
    ILoggerFactory loggerFactory)
    : SpecificAIServicePool<OpenAIConfiguration>(openAIConfiguration, customKernelBuilderConfig)
{
    protected override void RegisterChatCompletionService(IKernelBuilder kernelBuilder, OpenAIConfiguration config)
    {
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: config.ModelId,
            apiKey: config.ApiKey,
            orgId: config.OrgId, // Optional
            serviceId: config.ServiceId, // Optional; for targeting specific services within Semantic Kernel
            httpClient: HttpClient); // Optional; if not provided, the HttpClient from the kernel will be used
    }

    protected override ILogger Logger { get; } = loggerFactory.CreateLogger<OpenAIKernelPool>();
}