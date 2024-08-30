using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.ServicePools;

class HuggingFaceKernelPool : SpecificAIServicePool
{
    public HuggingFaceKernelPool(HuggingFaceConfiguration huggingFaceConfiguration, CustomKernelBuilderConfig customKernelBuilderConfig)
        : base(huggingFaceConfiguration, customKernelBuilderConfig)
    {
    }
}