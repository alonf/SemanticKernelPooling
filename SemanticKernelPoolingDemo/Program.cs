using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Polly;
using SemanticKernelPooling;
using SemanticKernelPooling.Connectors.Google;
using SemanticKernelPooling.Connectors.HuggingFace;
using SemanticKernelPooling.Connectors.MistralAI;
using SemanticKernelPooling.Connectors.OpenAI;
using SemanticKernelPoolingDemo;

var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
// Register the configuration instance to make it available for dependency injection
services.AddSingleton<IConfiguration>(configuration);

services.UseSemanticKernelPooling(); // Core service pooling registration


var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

RegisterServicePools();

var kernelPoolManager = serviceProvider.GetRequiredService<IKernelPoolManager>();

kernelPoolManager.RegisterForPreKernelCreation("math", (kernelBuilder, _, _, scopes) =>
{
#pragma warning disable SKEXP0050
    if (scopes.Contains("math"))
        kernelBuilder.Plugins.AddFromType<MathPlugin>("MathPlugin");
#pragma warning restore SKEXP0050
});

Random jitterer = new();

AsyncPolicy httpTimeoutAnd429RetryPolicy = Policy
    .Handle<Exception>(ex => ex.IsTransientError())
    .WaitAndRetryAsync(
        6, // Number of retries
        retryAttempt => TimeSpan.FromSeconds(Math.Min(40, 1 + Math.Pow(2, 1 + retryAttempt))) // Exponential backoff with a cap of 40 seconds, starting from 3-6 seconds
                        + TimeSpan.FromMilliseconds(jitterer.Next(0, 3000)), // Jitter between 0-3000 milliseconds
        (exception, timespan, retryCount, _) =>
        {
            // Log the error, retry attempt, and the calculated wait time
            logger.LogError(exception, "Kernel invocation retry {retryCount} due to {exceptionType}: {message}. Waiting {duration} seconds before next retry.",
                retryCount, exception.GetType().Name, exception.Message, timespan.TotalSeconds);
        });


// Create tasks for each provider
var tasks = new List<Task>();

for (int i = 0; i < 20; i++) // 4 tasks per kernel type
{
    var index = i; //to capture the index and not the loop variable
    await httpTimeoutAnd429RetryPolicy.ExecuteAsync(() =>
    {
        tasks.Add(RunKernelTaskAsync(index + 1));
        return Task.CompletedTask;
    }).ConfigureAwait(false);
}


// Run all tasks concurrently and wait for them to complete
await Task.WhenAll(tasks);

Console.WriteLine("All tasks completed.");

async Task RunKernelTaskAsync(int taskId)
{
    try
    {
        Console.WriteLine($"[Task {taskId}] Fetching kernel for scope: math");
        using var kernelWrapper = await kernelPoolManager.GetKernelByScopeAsync("math").ConfigureAwait(false);

        // Define an inline prompt directly in the code
        string prompt = "Solve: 2 + 2 * 2 =";

        // Create the function from the prompt
        var inlineFunction = kernelWrapper.Kernel.CreateFunctionFromPrompt(prompt);

        // Execute the inline prompt
        var response = await kernelWrapper.Kernel.InvokeAsync(inlineFunction).ConfigureAwait(false);
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