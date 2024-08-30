using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling;

public class KernelBuilderFactoryRegistry
{
    private readonly Dictionary<string, Func<AIServiceProviderConfiguration, IKernelBuilder>> _factoryMethods = new();

    public void RegisterFactory(string serviceType, Func<AIServiceProviderConfiguration, IKernelBuilder> factoryMethod)
    {
        _factoryMethods[serviceType] = factoryMethod;
    }

    public IKernelBuilder CreateBuilder(AIServiceProviderConfiguration aiServiceProviderConfiguration)
    {
        if (!_factoryMethods.TryGetValue(aiServiceProviderConfiguration.ServiceType, out var factoryMethod))
            throw new KeyNotFoundException($"No factory method registered for service type {aiServiceProviderConfiguration.ServiceType}");

        return factoryMethod(aiServiceProviderConfiguration);
    }

    private void RegisterFactoryMethods()
    {
        RegisterFactory("AzureOpenAI", configuration =>
        {

        });
    }
}