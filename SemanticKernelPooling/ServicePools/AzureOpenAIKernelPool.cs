using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.ServicePools;

class AzureOpenAIKernelPool : SpecificAIServicePool<AzureOpenAIConfiguration>
{
    public AzureOpenAIKernelPool(AzureOpenAIConfiguration azureOpenAIConfiguration, 
        CustomKernelBuilderConfig customKernelBuilderConfig,
        ILoggerFactory loggerFactory)
        : base(azureOpenAIConfiguration, customKernelBuilderConfig)
    {
        Logger = loggerFactory.CreateLogger<AzureOpenAIKernelPool>();
    }

    protected override Task RegisterChatCompletionServiceAsync(IKernelBuilder kernelBuilder, AzureOpenAIConfiguration config)
    {
        kernelBuilder.AddChatCompletionService(config);
    }

    protected override ILogger Logger { get; }
}