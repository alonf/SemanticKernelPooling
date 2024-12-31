using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernelPooling.Tests.Mocks;

/// <summary>
/// Provides a mock implementation of the kernel pool for testing purposes.
/// This implementation simulates the behavior of a real kernel pool without requiring actual AI service connections.
/// </summary>
/// <remarks>
/// <para>
/// The mock kernel pool tracks kernel creation and provides a controllable environment for testing
/// kernel pool behaviors including creation, disposal, and reuse patterns.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var config = new MockAIConfiguration { ... };
/// var pool = new MockKernelPool(config, loggerFactory, "path/to/responses");
/// </code>
/// </para>
/// </remarks>
public class MockKernelPool : AIServicePool<MockAIConfiguration>
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _responsesPath;
    private int _kernelCreationCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockKernelPool"/> class.
    /// </summary>
    /// <param name="config">The configuration for the mock kernel pool.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="responsesPath">The path to mock response files. Defaults to "MockResponses".</param>
    public MockKernelPool(
        MockAIConfiguration config,
        ILoggerFactory loggerFactory,
        string responsesPath = "MockResponses")
        : base(config)
    {
        _loggerFactory = loggerFactory;
        _responsesPath = responsesPath;
    }

    /// <summary>
    /// Registers a mock chat completion service with the kernel builder.
    /// </summary>
    /// <param name="kernelBuilder">The kernel builder to configure.</param>
    /// <param name="config">The mock configuration to use.</param>
    /// <param name="httpClient">Optional HTTP client to use. If null, a mock client will be created.</param>
    protected override void RegisterChatCompletionService(
        IKernelBuilder kernelBuilder,
        MockAIConfiguration config,
        HttpClient? httpClient)
    {
        Interlocked.Increment(ref _kernelCreationCount);

        if (httpClient == null)
        {
            var handler = new MockHttpMessageHandler(
                _responsesPath,
                _loggerFactory.CreateLogger<MockHttpMessageHandler>());
            httpClient = new HttpClient(handler);
        }

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: "mock-deployment",
            endpoint: "https://mock.openai.azure.com",
            serviceId: config.ServiceId,
            apiKey: config.ApiKey,
            httpClient: httpClient);
    }

    /// <inheritdoc/>
    protected override ILogger Logger => _loggerFactory.CreateLogger<MockKernelPool>();

    /// <summary>
    /// Gets the number of kernels that have been created by this pool.
    /// </summary>
    public int KernelCreationCount => _kernelCreationCount;
}