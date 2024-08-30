using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.ServicePools;

class OpenAIKernelPool : SpecificAIServicePool
{
    public OpenAIKernelPool(OpenAIConfiguration openAIConfiguration, CustomKernelBuilderConfig customKernelBuilderConfig)
        : base(openAIConfiguration, customKernelBuilderConfig)
    {
    }
}