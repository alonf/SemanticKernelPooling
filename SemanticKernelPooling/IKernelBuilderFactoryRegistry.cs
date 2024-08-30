using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling;

public interface IKernelBuilderFactoryRegistry
{
    void RegisterFactory(string serviceType, Func<AIServiceProviderConfiguration, IKernelBuilder> factoryMethod);
    IKernelBuilder CreateBuilder(AIServiceProviderConfiguration aiServiceProviderConfiguration);
}