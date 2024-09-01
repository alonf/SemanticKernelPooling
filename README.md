# SemanticKernelPooling

SemanticKernelPooling is a .NET library designed to facilitate seamless integration with multiple AI service providers, such as OpenAI, Azure OpenAI, HuggingFace, Google, Mistral AI, and others. It utilizes a kernel pooling approach to manage resources efficiently and provide robust AI capabilities in your .NET applications.

## Features

- **Kernel Pooling:** Efficiently manage and reuse kernels for different AI service providers.
- **Support for Multiple Providers:** Integrates with various AI providers like OpenAI, Azure OpenAI, HuggingFace, Google, Mistral AI, and more.
- **Extensibility:** Easily extendable to support additional AI service providers.
- **Customizable Configuration:** Allows fine-tuning of kernel behavior and AI service integration settings.
- **Logging Support:** Integrated with `Microsoft.Extensions.Logging` for detailed logging and diagnostics.
- **Error Handling and Retry Logic:** Implements robust error handling using Polly for retry policies, especially useful for managing API quotas and transient errors.

## Getting Started

### Prerequisites

- .NET 8.0 or higher
- NuGet packages:
  - `Microsoft.Extensions.DependencyInjection`
  - `Microsoft.Extensions.Logging`
  - `Microsoft.SemanticKernel`
  - `Polly` for advanced retry logic

### Installation

To install SemanticKernelPooling, you can use the NuGet package manager:

```bash
dotnet add package SemanticKernelPooling
```

### Basic Usage

1. **Configure Services**

   Start by configuring the services in your `Program.cs` or `Startup.cs` file:

   ```csharp
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   using SemanticKernelPooling;
   using SemanticKernelPooling.Connectors.OpenAI;

   var services = new ServiceCollection();
   services.AddLogging(configure => configure.AddConsole());
   services.UseSemanticKernelPooling(); // Core service pooling registration
   services.UseOpenAIKernelPool();      // Register OpenAI kernel pool
   services.UseAzureOpenAIKernelPool(); // Register Azure OpenAI kernel pool

   var serviceProvider = services.BuildServiceProvider();
   ```

2. **Configure Providers**

   You need to set up configuration settings for each AI service provider you intend to use. These settings can be defined in a `appsettings.json` or any configuration source supported by .NET:

   ```json
   {
     "AIServiceProviderConfigurations": [
       {
         "UniqueName": "OpenAI",
         "ServiceType": "OpenAI",
         "ApiKey": "YOUR_OPENAI_API_KEY",
         "ModelId": "YOUR_MODEL_ID"
       },
       {
         "UniqueName": "AzureOpenAI",
         "ServiceType": "AzureOpenAI",
         "DeploymentName": "YOUR_DEPLOYMENT_NAME",
         "ApiKey": "YOUR_AZURE_API_KEY",
         "Endpoint": "YOUR_ENDPOINT",
         "ModelId": "YOUR_MODEL_ID",
         "ServiceId": "YOUR_SERVICE_ID"
       }
       // Add more providers as needed
     ]
   }
   ```

3. **Retrieve a Kernel and Execute Commands**

   Once the service providers are configured and registered, you can retrieve a kernel from the pool and execute commands:

   ```csharp
   var kernelPoolManager = serviceProvider.GetRequiredService<IKernelPoolManager>();

   // Example: Getting a kernel for OpenAI
   using var kernelWrapper = await kernelPoolManager.GetKernelAsync(AIServiceProviderType.OpenAI);

   // Use the kernel to perform AI operations
   var response = await kernelWrapper.Kernel.ExecuteAsync("What is Semantic Kernel?");
   Console.WriteLine(response);

   // Return the kernel to the pool after use
   ```

### Advanced Usage

4. **Using Retry Policies**

   To handle API rate limits and transient errors, use Polly to define retry policies:

   ```csharp
   AsyncPolicy httpTimeoutAndRetryPolicy = Policy
       .Handle<Exception>(ex => ex.IsTransientError())
       .WaitAndRetryAsync(
           retryCount: 6,
           sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(new Random().Next(0, 3000)),
           onRetry: (exception, timespan, retryCount, context) =>
           {
               logger.LogError($"Retry {retryCount} after {timespan.TotalSeconds} seconds due to: {exception.Message}");
           });
   ```

5. **Adding New AI Providers**

   To add support for a new AI provider, follow these steps:

   - **Create a Configuration Class:** Define a new configuration class inheriting from `AIServiceProviderConfiguration`.

   - **Implement a Kernel Pool Class:** Create a new kernel pool class inheriting from `AIServicePool<T>`.

   - **Register the New Provider:** Add the registration method in the `ServiceExtension` class to register your new provider with the DI container.

   For example, to add a new "CustomAI" provider:

   ```csharp
   public record CustomAIConfiguration : AIServiceProviderConfiguration
   {
       public required string ModelId { get; init; }
       public required string ApiKey { get; init; }
       // Additional settings...
   }

   class CustomAIKernelPool(
       CustomAIConfiguration config,
       ILoggerFactory loggerFactory)
       : AIServicePool<CustomAIConfiguration>(config)
   {
       protected override void RegisterChatCompletionService(IKernelBuilder kernelBuilder, CustomAIConfiguration config, HttpClient? httpClient)
       {
           // Register service logic...
       }

       protected override ILogger Logger { get; } = loggerFactory.CreateLogger<CustomAIKernelPool>();
   }

   public static class ServiceExtension
   {
       public static void UseCustomAIKernelPool(this IServiceProvider serviceProvider)
       {
           var registrar = serviceProvider.GetRequiredService<IKernelPoolFactoryRegistrar>();
           registrar.RegisterKernelPoolFactory(
               AIServiceProviderType.CustomAI,
               (aiServiceProviderConfiguration, loggerFactory) =>
                   new CustomAIKernelPool((CustomAIConfiguration)aiServiceProviderConfiguration, loggerFactory));
       }
   }
   ```

## Supported Providers

- **OpenAI:** Use `OpenAIConfiguration` and `OpenAIKernelPool` to interact with OpenAI services.
- **Azure OpenAI:** Use `AzureOpenAIConfiguration` and `AzureOpenAIKernelPool` for Azure OpenAI.
- **HuggingFace:** Use `HuggingFaceConfiguration` and `HuggingFaceKernelPool` to integrate with HuggingFace models.
- **Google AI:** Use `GoogleConfiguration` and `GoogleKernelPool` for Google AI services.
- **Mistral AI:** Use `MistralAIConfiguration` and `MistralAIKernelPool` to leverage Mistral AI services.
- **Custom Providers:** Easily extend to support other providers by following the extensibility guidelines.

## Contributing

Contributions are welcome! Please fork the repository, make your changes, and submit a pull request. Ensure your code adheres to the project's coding standards and includes appropriate tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Acknowledgments

- Special thanks to the contributors of Microsoft Semantic Kernel and all integrated AI service providers.
- Inspired by the need for efficient AI resource management in .NET applications.
