# SK Pool Library

**SK Pool Library** is a .NET library designed to efficiently manage a pool of Semantic Kernel (SK) instances for AI services, including Azure OpenAI, OpenAI, Hugging Face, and more. The library provides robust, thread-safe, and scalable management of multiple kernel instances, enabling developers to optimize resource usage and improve the performance of AI-driven applications.

## Features

- **Kernel Pool Initialization**: Initialize a pool of kernels with multiple configurations, supporting various AI service endpoints like Azure OpenAI, OpenAI, and Hugging Face.
- **General and Specific Pre/Post Kernel Actions**: Register both general actions (e.g., setting up SK functions, memory) and service-specific actions to customize kernel initialization and teardown.
- **Flexible Kernel Retrieval**: Retrieve kernels based on type, endpoint, or other criteria using a simple, intuitive API.
- **Round-Robin Kernel Allocation**: Distribute kernel usage evenly across all instances to balance the load and ensure optimal performance.
- **Concurrency Control**: Manage concurrent access to kernel instances using thread-safe mechanisms like `SemaphoreSlim` to prevent conflicts and ensure reliable operations.
- **Automatic Resource Management**: Implement `IDisposable` and other resource management patterns to ensure kernels are returned to the pool after use, preventing leaks and ensuring efficient reuse.
- **Dynamic Pool Scaling**: Adjust the number of kernels in the pool dynamically based on application demand, allowing for scalable and flexible resource management.
- **Comprehensive Error Handling**: Incorporate robust error handling strategies, including retry logic with Polly, to build resilient AI applications.
- **Logging and Monitoring**: Integrate with common logging frameworks to provide detailed logs, monitoring, and observability of kernel usage, errors, and performance metrics.
- **Custom Kernel Support**: Extend or override kernel creation logic to integrate custom kernels with specialized services or configurations.

## Getting Started

### Prerequisites

- .NET 8.0 or .NET Standard 2.0
- NuGet Package Manager

### Installation

You can install the SK Pool Library via NuGet Package Manager:

```bash
dotnet add package SKPoolLibrary
```

### Basic Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SKPoolLibrary;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var kernelPoolManager = new GenericKernelPoolManager<Kernel>(
            configurations: GetKernelConfigurations(),
            kernelFactory: new KernelFactory(),
            logger: GetLogger());

        // Register general actions for kernel creation
        kernelPoolManager.RegisterForPreKernelCreation((builder, config) =>
        {
            builder.Services.AddMemory();
            builder.Services.AddTextCompletion();
        });

        // Retrieve a kernel by type
        var kernel = await kernelPoolManager.GetKernelAsync(typeof(AzureOpenAIKernel));
        // Use the kernel...
        kernel.Dispose(); // Return the kernel to the pool
    }
}
```

### Configuration

The SK Pool Library allows flexible configuration of kernel instances. You can configure kernels for different AI services by providing custom configurations:

```csharp
var azureOpenAIConfig = new AzureOpenAIConfiguration
{
    UniqueName = "AzureEndpoint1",
    Endpoint = "https://api.openai.com/v1/engines/davinci",
    ApiKey = "YOUR_API_KEY",
    DeploymentName = "text-davinci-003"
};

// Add multiple configurations
var configurations = new List<IKernelConfiguration> { azureOpenAIConfig, openAIConfig };
```

### Advanced Usage

#### Registering Service-Specific Actions

```csharp
kernelPoolManager.RegisterForSpecificPreKernelCreation(typeof(OpenAIKernel), (builder, config) =>
{
    // Add service-specific configurations
});

kernelPoolManager.RegisterForSpecificAfterKernelCreation(typeof(OpenAIKernel), (kernel, config) =>
{
    // Perform post-creation tasks
});
```

### Contributing

We welcome contributions to the SK Pool Library! Feel free to submit issues, fork the repository, and create pull requests. Please ensure all changes are well-documented and covered by unit tests.

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Acknowledgments

This library was inspired by the need to efficiently manage AI service connections and resource usage in high-concurrency environments. Special thanks to the developers and maintainers of the Semantic Kernel SDK for providing a robust foundation for AI model integration.

