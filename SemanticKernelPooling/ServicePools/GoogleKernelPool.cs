using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.ServicePools;

class GoogleKernelPool : SpecificAIServicePool
{
    public GoogleKernelPool(GoogleConfiguration googleConfiguration, CustomKernelBuilderConfig customKernelBuilderConfig)
        : base(googleConfiguration, customKernelBuilderConfig)
    {
    }
}