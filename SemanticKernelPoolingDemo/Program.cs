using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using SemanticKernelPooling;
using SemanticKernelPooling.Connectors.Google;
using SemanticKernelPooling.Connectors.HuggingFace;
using SemanticKernelPooling.Connectors.MistralAI;
using SemanticKernelPooling.Connectors.OpenAI;

var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

var services = new ServiceCollection();
services.AddSingleton<IKernelPoolFactoryRegistrar, KernelPoolFactoryRegistrar>();
services.AddLogging(configure => configure.AddConsole());
// Register the configuration instance to make it available for dependency injection
services.AddSingleton<IConfiguration>(configuration);

services.UseSemanticKernelPooling(); // Core service pooling registration


var serviceProvider = services.BuildServiceProvider();

RegisterServicePools();

var kernelPoolManager = serviceProvider.GetRequiredService<IKernelPoolManager>();

kernelPoolManager.RegisterForPreKernelCreation("math", (kernelBuilder, _, _, scopes) =>
{
#pragma warning disable SKEXP0050
    if (scopes.Contains("math"))
        kernelBuilder.Plugins.AddFromType<MathPlugin>("MathPlugin");
#pragma warning restore SKEXP0050
});


// Create tasks for each provider
var tasks = new List<Task>();

for (int i = 0; i < 20; i++) // 4 tasks per kernel type
{
    tasks.Add(RunKernelTask(i + 1));
}


// Run all tasks concurrently and wait for them to complete
await Task.WhenAll(tasks);

Console.WriteLine("All tasks completed.");

async Task RunKernelTask(int taskId)
{
    try
    {
        Console.WriteLine($"[Task {taskId}] Fetching kernel for scope: math");
        using var kernelWrapper = await kernelPoolManager.GetKernelAsync("math");

        // Define an inline prompt directly in the code
        string prompt = "What is 2 + 2 * 2?";

        // Create the function from the prompt
        var inlineFunction = kernelWrapper.Kernel.CreateFunctionFromPrompt(prompt);

        // Execute the inline prompt
        var response = await kernelWrapper.Kernel.InvokeAsync(inlineFunction);
        Console.WriteLine($"[Task {taskId}] {kernelWrapper.ServiceProviderType} result: {response}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Task {taskId}] Error: {ex.Message}");
    }
}

void RegisterServicePools()
{
    serviceProvider.UseOpenAIKernelPool();
    serviceProvider.UseAzureOpenAIKernelPool();
    serviceProvider.UseHuggingFaceKernelPool();
    serviceProvider.UseGoogleKernelPool();
    serviceProvider.UseMistralAIKernelPool();
}