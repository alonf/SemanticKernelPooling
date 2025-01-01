using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemanticKernelPooling.Tests.Mocks;
using SemanticKernelPooling.Tests.Unit.Logging;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit;

/// <summary>
/// Test fixture for Semantic Kernel Pool tests that provides configured services,
/// logging, and mock response handling capabilities.
/// </summary>
public class SemanticKernelPoolTestFixture : IDisposable
{
    /// <summary>
    /// Gets the configured service provider containing all required services for testing.
    /// </summary>
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>
    /// Gets the test output helper for logging test information.
    /// </summary>
    public ITestOutputHelper TestOutputHelper
    {
        get
        {
            if (_testOutputHelper == null)
                throw new InvalidOperationException("TestOutputHelper not configured. Call ConfigureLogging first.");
            return _testOutputHelper;
        }
        private set => _testOutputHelper = value;
    }

    private readonly string _mockResponsesPath;
    private ILoggerProvider? _loggerProvider;
    private ITestOutputHelper? _testOutputHelper;

    /// <summary>
    /// The scope used for test configurations.
    /// </summary>
    public string TestScope = "test-scope";

    /// <summary>
    /// The size of the kernel pool for testing.
    /// </summary>
    public int PoolSize = 2;

    /// <summary>
    /// The mock deployment name used in test configurations.
    /// </summary>
    public const string DeploymentName = "mock-deployment";

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticKernelPoolTestFixture"/> class.
    /// </summary>
    public SemanticKernelPoolTestFixture()
    {
        _mockResponsesPath = Path.Combine(Path.GetTempPath(), "MockResponses");
        Directory.CreateDirectory(_mockResponsesPath);
        InitializeServices();
    }

    public void InitializeServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.UseSemanticKernelPooling();

        ServiceProvider = services.BuildServiceProvider();
        ServiceProvider.UseMockKernelPool(_mockResponsesPath);
    }

    /// <summary>
    /// Configures logging for a specific test, ensuring test output is properly captured.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper provided by XUnit.</param>
    public void ConfigureLogging(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;  // Set the property
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

        _loggerProvider?.Dispose();
        _loggerProvider = new XUnitLoggerProvider(testOutputHelper);
        loggerFactory.AddProvider(_loggerProvider);

        TestOutputHelper.WriteLine("Logging configured successfully");
    }

    /// <summary>
    /// Adds a mock response for API calls during testing.
    /// </summary>
    /// <param name="requestKey">The key identifying the request to mock.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="content">The content to return in the response.</param>
    public void AddMockResponse(string requestKey, HttpStatusCode statusCode, string content)
    {
        TestOutputHelper.WriteLine($"Adding mock response for request key: {requestKey}");

        var response = new MockResponse
        {
            RequestKey = requestKey,
            StatusCode = statusCode,
            Content = content,
            Headers = new Dictionary<string, string[]>
            {
                { "Content-Type", new[] { "application/json" } }
            }
        };

        var filePath = Path.Combine(_mockResponsesPath, $"{Guid.NewGuid()}.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(response));
        TestOutputHelper.WriteLine($"Mock response saved to: {filePath}");
    }

    /// <summary>
    /// Performs cleanup of managed and unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _loggerProvider?.Dispose();

        try
        {
            if (Directory.Exists(_mockResponsesPath))
            {
                Directory.Delete(_mockResponsesPath, true);
            }
        }
        catch (Exception ex)
        {
            // Since we're disposing, we might not have access to TestOutputHelper,
            // but we'll try to log if we can
            try
            {
                TestOutputHelper.WriteLine($"Error cleaning up mock responses directory: {ex.Message}");
            }
            catch
            {
                // Nothing we can do here if TestOutputHelper is not available
            }
        }
    }
}