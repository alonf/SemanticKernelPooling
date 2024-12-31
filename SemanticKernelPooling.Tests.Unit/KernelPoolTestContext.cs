using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SemanticKernelPooling.Tests.Mocks;
using System.Net;
using System.Text.Json;

namespace SemanticKernelPooling.Tests.Unit;

/// <summary>
/// Provides a shared test context for kernel pool tests, managing mock configurations
/// and service setup that can be reused across multiple test cases.
/// </summary>
/// <remarks>
/// <para>
/// This context handles the initialization of mock services, configuration, and response management
/// required for testing kernel pool functionality without real service dependencies.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// public class MyTests : IClassFixture&lt;KernelPoolTestContext&gt;
/// {
///     private readonly KernelPoolTestContext _context;
///     
///     public MyTests(KernelPoolTestContext context)
///     {
///         _context = context;
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public class KernelPoolTestContext : IDisposable
{
    /// <summary>
    /// Gets the configured service provider for test cases.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    private readonly ServiceProvider _disposableServiceProvider;
    private readonly string _mockResponsesPath;
    public const string TestScope = "test-scope";

    /// <summary>
    /// Initializes a new instance of the <see cref="KernelPoolTestContext"/> class.
    /// Sets up the mock configuration, services, and response directory needed for testing.
    /// </summary>
    public KernelPoolTestContext()
    {
        _mockResponsesPath = Path.Combine(Path.GetTempPath(), "MockResponses");
        Directory.CreateDirectory(_mockResponsesPath);

        // Create mock configuration
        var mockConfigurationSection = new Mock<IConfigurationSection>();
        mockConfigurationSection.Setup(x => x.GetChildren())
            .Returns(new List<IConfigurationSection>
            {
                CreateMockConfigSection("MockOpenAI", AIServiceProviderType.OpenAI, new[] { TestScope }),
                CreateMockConfigSection("MockAzureOpenAI", AIServiceProviderType.AzureOpenAI, new[] { TestScope })
            });

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration
            .Setup(x => x.GetSection("AIServiceProviderConfigurations"))
            .Returns(mockConfigurationSection.Object);

        // Set up services
        var services = new ServiceCollection();
        services.AddSingleton(mockConfiguration.Object);
        services.UseSemanticKernelPooling();

        _disposableServiceProvider = services.BuildServiceProvider();
        _disposableServiceProvider.UseMockKernelPool(_mockResponsesPath);

        ServiceProvider = _disposableServiceProvider;
    }

    /// <summary>
    /// Adds a mock HTTP response to be used in tests.
    /// </summary>
    /// <param name="requestKey">The request key to match for this response (e.g., "POST:/v1/chat/completions").</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="content">The response content to return.</param>
    /// <remarks>
    /// <para>
    /// The mock response will be saved as a JSON file and used by the mock HTTP handler
    /// when matching requests during tests.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// context.AddMockResponse(
    ///     "POST:/v1/chat/completions",
    ///     HttpStatusCode.OK,
    ///     "{\"choices\": [{\"message\": {\"content\": \"Test response\"}}]}");
    /// </code>
    /// </para>
    /// </remarks>
    public void AddMockResponse(string requestKey, HttpStatusCode statusCode, string content)
    {
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
    }

    /// <summary>
    /// Creates a mock configuration section for testing kernel pool functionality.
    /// </summary>
    /// <param name="uniqueName">The unique name for identifying the configuration instance.</param>
    /// <param name="serviceType">The type of AI service provider (e.g., OpenAI, AzureOpenAI).</param>
    /// <param name="scopes">An array of scope names associated with this configuration.</param>
    /// <returns>A mocked <see cref="IConfigurationSection"/> with all required configuration values.</returns>
    /// <remarks>
    /// <para>
    /// This method creates a mock configuration section that simulates the structure and values
    /// that would typically come from appsettings.json or other configuration sources.
    /// </para>
    /// <para>
    /// The mock configuration includes all required properties for both standard and Azure-specific
    /// configurations, ensuring compatibility with different service provider types.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var configSection = CreateMockConfigSection(
    ///     "TestOpenAI",
    ///     AIServiceProviderType.OpenAI,
    ///     new[] { "test-scope" });
    /// </code>
    /// </para>
    /// </remarks>
    /// <seealso cref="IConfigurationSection"/>
    /// <seealso cref="AIServiceProviderType"/>
    private IConfigurationSection CreateMockConfigSection(
        string uniqueName,
        AIServiceProviderType serviceType,
        string[] scopes)
    {
        ArgumentNullException.ThrowIfNull(uniqueName);
        ArgumentNullException.ThrowIfNull(scopes);

        var mockSection = new Mock<IConfigurationSection>();

        // Setup AIServiceProviderConfiguration base properties
        mockSection.Setup(x => x.GetValue<string>("UniqueName"))
            .Returns(uniqueName);
        mockSection.Setup(x => x.GetValue<AIServiceProviderType>("ServiceType"))
            .Returns(serviceType);
        mockSection.Setup(x => x.GetSection("Scopes").Get<List<string>>())
            .Returns(scopes.ToList());
        mockSection.Setup(x => x.GetValue<int>("InstanceCount"))
            .Returns(2);
        mockSection.Setup(x => x.GetValue<string>("DeploymentTextEmbedding"))
            .Returns("mock-embedding");
        mockSection.Setup(x => x.GetValue<int>("MaxWaitForKernelInSeconds"))
            .Returns(30);

        // Setup Mock-specific properties
        mockSection.Setup(x => x.GetValue<string>("ApiKey"))
            .Returns("mock-api-key");
        mockSection.Setup(x => x.GetValue<string>("ModelId"))
            .Returns("mock-model");
        mockSection.Setup(x => x.GetValue<string>("ServiceId"))
            .Returns("mock-service-id");
        mockSection.Setup(x => x.GetValue<string>("Endpoint"))
            .Returns("https://mock.endpoint");

        return mockSection.Object;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// Cleans up both the service provider and any mock response files created during testing.
    /// </remarks>
    public void Dispose()
    {
        _disposableServiceProvider?.Dispose();

        try
        {
            if (Directory.Exists(_mockResponsesPath))
            {
                Directory.Delete(_mockResponsesPath, true);
            }
        }
        catch (Exception)
        {
            // Log or handle cleanup failure
        }
    }
}