using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.ServicePools;

class MistralKernelPool : SpecificAIServicePool
{
    public MistralKernelPool(AIServiceProviderConfiguration aiServiceProviderConfiguration, CustomKernelBuilderConfig customKernelBuilderConfig)
        : base(aiServiceProviderConfiguration, customKernelBuilderConfig)
    {
    }
}