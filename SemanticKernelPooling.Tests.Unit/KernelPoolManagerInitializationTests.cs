using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Tests.Mocks;
using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit;

[Collection("KernelPoolManagerTests")]
public class KernelPoolManagerInitializationTests
{
    private IServiceProvider _serviceProvider => _fixture.ServiceProvider;
    private readonly SemanticKernelPoolTestFixture _fixture;
    private ITestOutputHelper OutputHelper => _fixture.TestOutputHelper;
    private string TestScope1 => _fixture.TestScope1;
    private string TestScope2 => _fixture.TestScope2;

    public KernelPoolManagerInitializationTests(SemanticKernelPoolTestFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _fixture.ConfigureLogging(outputHelper);
        _fixture.InitializeServices();
        OutputHelper.WriteLine("Initializing KernelPoolManagerInitializationTests");
    }

    [Fact]
    public async Task RegisterForPreKernelCreation_WithScope_ExecutesForMatchingScope()
    {
        // Arrange
        OutputHelper.WriteLine($"Starting test: RegisterForPreKernelCreation with scope '{TestScope1}'");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var executionCount = 0;

        // Register scoped pre-initialization action
        kernelPoolManager.RegisterForPreKernelCreation(TestScope1,
            (builder, config, options, scopes) =>
            {
                OutputHelper.WriteLine("Executing preRegisteration");
                Assert.NotNull(scopes); // Verify scopes is not null
                Assert.Contains(TestScope1, scopes);
                Interlocked.Increment(ref executionCount);
                OutputHelper.WriteLine($"Pre-initialization action executed for scopes: {string.Join(", ", scopes)}");
            });

        // Act
        using var wrapper1 = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);
        using var wrapper2 = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);

        using var wrapperDifferentScope = await kernelPoolManager.GetKernelByScopeAsync(TestScope2);

        OutputHelper.WriteLine("wrapper 1 unique name: " + wrapper1.UniqueName);
        OutputHelper.WriteLine("wrapper 2 unique name: " + wrapper2.UniqueName);
        OutputHelper.WriteLine("wrapper Different Scope unique name: " + wrapperDifferentScope.UniqueName);
        
        // Assert
        OutputHelper.WriteLine($"Execution count: {executionCount}");
        Assert.Equal(2, executionCount);
        Assert.DoesNotContain(TestScope1, wrapperDifferentScope.Scopes);
    }

    [Fact]
    public async Task RegisterForAfterKernelCreation_WithScope_ExecutesForMatchingScope()
    {
        // Arrange
        OutputHelper.WriteLine($"Starting test: RegisterForAfterKernelCreation with scope '{TestScope1}'");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var executionCount = 0;
        Kernel? lastConfiguredKernel = null;

        // Register scoped post-initialization action
        kernelPoolManager.RegisterForAfterKernelCreation(TestScope1,
            (kernel, config, scopes) =>
            {
                OutputHelper.WriteLine("Executing AfterRegisteration");
                Assert.NotNull(scopes); // Verify scopes is not null
                Assert.Contains(TestScope1, scopes);
                Interlocked.Increment(ref executionCount);
                lastConfiguredKernel = kernel;
                OutputHelper.WriteLine($"Post-initialization action executed for scopes: {string.Join(", ", scopes)}");
            });

        // Act
        using var wrapper1 = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);
        using var wrapper2 = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);

        using var wrapperDifferentScope= await kernelPoolManager.GetKernelByScopeAsync(TestScope2);

        OutputHelper.WriteLine("wrapper 1 unique name: " + wrapper1.UniqueName);
        OutputHelper.WriteLine("wrapper 2 unique name: " + wrapper2.UniqueName);
        OutputHelper.WriteLine("wrapper Different Scope unique name: " + wrapperDifferentScope.UniqueName);

        // Assert
        OutputHelper.WriteLine($"Execution count: {executionCount}");
        Assert.Equal(2, executionCount);
        Assert.Same(lastConfiguredKernel, wrapper2.Kernel);
        Assert.DoesNotContain(TestScope1, wrapperDifferentScope.Scopes);
    }

    [Fact(Skip ="fix register by configuration type")]
    public async Task RegisterForPreKernelCreation_WithType_ExecutesForMatchingType()
    {
        // Arrange
        OutputHelper.WriteLine("Starting test: RegisterForPreKernelCreation with type MockAIConfiguration");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var providerType = AIServiceProviderType.OpenAI;
        var executionCount = 0;

        // Register type-specific pre-initialization action
        kernelPoolManager.RegisterForPreKernelCreation<MockAIConfiguration>(
            (builder, config, options) =>
            {
                OutputHelper.WriteLine("Executing preRegistration for MockAIConfiguration");
                Assert.IsType<MockAIConfiguration>(config);
                Interlocked.Increment(ref executionCount);
                OutputHelper.WriteLine($"Pre-initialization action executed for config: {config.UniqueName}");
            });

        // Act
        using var wrapper1 = await kernelPoolManager.GetKernelAsync(providerType);
        using var wrapper2 = await kernelPoolManager.GetKernelAsync(providerType);

        OutputHelper.WriteLine($"wrapper 1 type: {providerType}, unique name: {wrapper1.UniqueName}");
        OutputHelper.WriteLine($"wrapper 2 type: {providerType}, unique name: {wrapper2.UniqueName}");

        // Assert
        OutputHelper.WriteLine($"Execution count: {executionCount}");
        Assert.Equal(2, executionCount);
    }

    [Fact(Skip = "fix register by configuration type")]
    public async Task RegisterForAfterKernelCreation_WithType_ExecutesForMatchingType()
    {
        // Arrange
        OutputHelper.WriteLine("Starting test: RegisterForAfterKernelCreation with type MockAIConfiguration");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var providerType = AIServiceProviderType.OpenAI;
        var executionCount = 0;
        Kernel? lastConfiguredKernel = null;

        // Register type-specific post-initialization action
        kernelPoolManager.RegisterForAfterKernelCreation<MockAIConfiguration>(
            (kernel, config) =>
            {
                OutputHelper.WriteLine("Executing afterRegistration for MockAIConfiguration");
                Assert.IsType<MockAIConfiguration>(config);
                Interlocked.Increment(ref executionCount);
                lastConfiguredKernel = kernel;
                OutputHelper.WriteLine($"Post-initialization action executed for config: {config.UniqueName}");
            });

        // Act
        using var wrapper1 = await kernelPoolManager.GetKernelAsync(providerType);
        using var wrapper2 = await kernelPoolManager.GetKernelAsync(providerType);

        OutputHelper.WriteLine($"wrapper 1 type: {providerType}, unique name: {wrapper1.UniqueName}");
        OutputHelper.WriteLine($"wrapper 2 type: {providerType}, unique name: {wrapper2.UniqueName}");

        // Assert
        OutputHelper.WriteLine($"Execution count: {executionCount}");
        Assert.Equal(2, executionCount);
        Assert.Same(lastConfiguredKernel, wrapper2.Kernel);
    }
}