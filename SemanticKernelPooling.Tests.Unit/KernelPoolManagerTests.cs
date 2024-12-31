using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit;

/// <summary>
/// Test class for kernel pool manager functionality
/// </summary>
public class KernelPoolManagerTests : IClassFixture<SemanticKernelPoolTestFixture>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemanticKernelPoolTestFixture _fixture;
    private const string TestScope = "test-scope";
    private const int PoolSize = 2;
    private ITestOutputHelper OutputHelper => _fixture.TestOutputHelper;

    public KernelPoolManagerTests(SemanticKernelPoolTestFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _serviceProvider = fixture.ServiceProvider;
        _fixture.ConfigureLogging(outputHelper);
        OutputHelper.WriteLine("Initializing KernelPoolManagerTests");
        SetupMockResponses();
    }

    [Fact]
    public async Task GetKernelByScope_ReturnsValidKernel()
    {
        OutputHelper.WriteLine("Starting GetKernelByScope_ReturnsValidKernel test");

        // Arrange
        OutputHelper.WriteLine("Getting kernel pool manager from service provider");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();

        // Act
        OutputHelper.WriteLine($"Attempting to get kernel by scope: {TestScope}");
        using var kernelWrapper = await kernelPoolManager.GetKernelByScopeAsync(TestScope);

        // Assert
        OutputHelper.WriteLine("Verifying kernel wrapper properties");
        Assert.NotNull(kernelWrapper);
        Assert.NotNull(kernelWrapper.Kernel);
        Assert.Contains(TestScope, kernelWrapper.Scopes);
        OutputHelper.WriteLine("GetKernelByScope_ReturnsValidKernel test completed successfully");
    }

    [Fact]
    public async Task GetKernelByType_ReturnsValidKernel()
    {
        OutputHelper.WriteLine("Starting GetKernelByType_ReturnsValidKernel test");

        // Arrange
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();

        // Act
        OutputHelper.WriteLine($"Attempting to get kernel by type: {AIServiceProviderType.OpenAI}");
        using var kernelWrapper = await kernelPoolManager.GetKernelAsync(AIServiceProviderType.OpenAI);

        // Assert
        OutputHelper.WriteLine("Verifying kernel wrapper properties");
        Assert.NotNull(kernelWrapper);
        Assert.NotNull(kernelWrapper.Kernel);
        Assert.Equal(AIServiceProviderType.OpenAI, kernelWrapper.ServiceProviderType);
        OutputHelper.WriteLine("GetKernelByType test completed successfully");
    }

    [Fact]
    public async Task GetKernelByName_ReturnsCorrectKernel()
    {
        OutputHelper.WriteLine("Starting GetKernelByName_ReturnsCorrectKernel test");

        // Arrange
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        const string kernelName = "MockOpenAI";

        // Act
        OutputHelper.WriteLine($"Attempting to get kernel by name: {kernelName}");
        using var kernelWrapper = await kernelPoolManager.GetKernelByNameAsync(kernelName);

        // Assert
        OutputHelper.WriteLine("Verifying kernel wrapper properties");
        Assert.NotNull(kernelWrapper);
        Assert.NotNull(kernelWrapper.Kernel);
        Assert.Equal(AIServiceProviderType.OpenAI, kernelWrapper.ServiceProviderType);
        OutputHelper.WriteLine("GetKernelByName test completed successfully");
    }

    //[Fact]
    //public async Task MultipleKernels_RoundRobinSelection()
    //{
    //    OutputHelper.WriteLine("Starting MultipleKernels_RoundRobinSelection test");

    //    // Arrange
    //    var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
    //    var kernels = new HashSet<Kernel>();

    //    // Act
    //    OutputHelper.WriteLine($"Requesting {PoolSize} kernels to verify round-robin selection");
    //    for (int i = 0; i < PoolSize; i++)
    //    {
    //        OutputHelper.WriteLine($"Getting kernel {i + 1}");
    //        using var wrapper = await kernelPoolManager.GetKernelByScopeAsync(TestScope);
    //        kernels.Add(wrapper.Kernel);
    //    }

    //    // Assert
    //    OutputHelper.WriteLine("Verifying unique kernel count");
    //    Assert.Equal(PoolSize, kernels.Count);
    //    OutputHelper.WriteLine("Round-robin selection test completed successfully");
    //}

    private void SetupMockResponses()
    {
        OutputHelper.WriteLine("Setting up mock responses for chat completions");
        var mockResponse = @"{
            ""id"": ""mock-id"",
            ""object"": ""chat.completion"",
            ""created"": 1700000000,
            ""model"": ""mock-model"",
            ""choices"": [
                {
                    ""index"": 0,
                    ""message"": {
                        ""role"": ""assistant"",
                        ""content"": ""Mock response""
                    },
                    ""finish_reason"": ""stop""
                }
            ]
        }";

        _fixture.AddMockResponse("POST:/v1/chat/completions", HttpStatusCode.OK, mockResponse);
        OutputHelper.WriteLine("Mock responses setup completed");
    }
}