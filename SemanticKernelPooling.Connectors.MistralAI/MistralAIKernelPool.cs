using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;
using SemanticKernelPooling.ServicePools;

namespace SemanticKernelPooling.Connectors.MistralAI;
#pragma warning disable SKEXP0070

class MistralAIKernelPool(
        MistralAIConfiguration mistralAIConfiguration,
        CustomKernelBuilderConfig customKernelBuilderConfig,
        ILoggerFactory loggerFactory)
        : SpecificAIServicePool<MistralAIConfiguration>(mistralAIConfiguration, customKernelBuilderConfig)
    {
        protected override void RegisterChatCompletionService(IKernelBuilder kernelBuilder, MistralAIConfiguration config)
        {
            kernelBuilder.AddMistralChatCompletion(
                modelId: config.ModelId,
                apiKey: config.ApiKey,
                endpoint: string.IsNullOrEmpty(config.Endpoint) ? null : new Uri(config.Endpoint), // Optional
                serviceId: config.ServiceId, // Optional; for targeting specific services within Semantic Kernel
                httpClient: HttpClient); // Optional; if not provided, the HttpClient from the kernel will be used
        }

        protected override ILogger Logger { get; } = loggerFactory.CreateLogger<MistralAIKernelPool>();
    }