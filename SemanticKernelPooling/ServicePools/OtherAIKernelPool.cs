using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.ServicePools;

class OtherAIKernelPool : SpecificAIServicePool
{
    public OtherAIKernelPool(AIServiceProviderConfiguration aiServiceProviderConfiguration, CustomKernelBuilderConfig customKernelBuilderConfig)
        : base(aiServiceProviderConfiguration, customKernelBuilderConfig)
    {
    }
}