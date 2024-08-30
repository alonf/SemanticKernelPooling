using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var providerConfigs = configuration.GetSection("ServiceProviderConfigurations").Get<List<AIServiceProviderConfiguration>>();
        var logger = services.BuildServiceProvider().GetService<ILogger<Startup>>();

        if (providerConfigs == null || providerConfigs.Count == 0)
        {
            logger?.LogError("SemanticKernelPooling: No service provider configurations provided.");
            return;
        }

        services.AddSingleton(providerConfigs);
        services.AddSingleton<IKernelPool, KernelPoolManager>();
        services.AddSingleton<KernelBuilderFactoryRegistry>(sp =>
        {
            var registry = new KernelBuilderFactoryRegistry();

            // Register factory methods
            registry.RegisterFactory("AzureOpenAI", config =>
            {
                var azureConfig = (AzureOpenAIConfiguration)config;
                var kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.AddAzureOpenAIChatCompletion();
                return kernelBuilder.Build();
            });

            registry.RegisterFactory("OpenAI", config =>
            {
                var openAiConfig = (OpenAIConfiguration)config;
                var kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.WithOpenAIChatCompletionService(new OpenAIClient(openAiConfig.ApiKey));
                return kernelBuilder.Build();
            });

            // Add more registrations here...

            return registry;
        });
    }
}
