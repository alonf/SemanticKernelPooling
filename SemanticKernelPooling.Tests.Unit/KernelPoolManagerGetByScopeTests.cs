using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit;

[Collection("KernelPoolManagerTests")]
public class KernelPoolManagerGetByScopeTests
{
    private IServiceProvider _serviceProvider => _fixture.ServiceProvider;
    private readonly SemanticKernelPoolTestFixture _fixture;
    private ITestOutputHelper OutputHelper => _fixture.TestOutputHelper;
    private string TestScope1 => _fixture.TestScope1;
    private int PoolSize => _fixture.PoolSize;
    private int TotalPoolSize => PoolSize * 2;

    public KernelPoolManagerGetByScopeTests(SemanticKernelPoolTestFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _fixture.ConfigureLogging(outputHelper);
        _fixture.InitializeServices();
        OutputHelper.WriteLine("Initializing KernelPoolManagerGetByScopeTests");
    }

    [Fact]
    public async Task GetKernelByScope_WithValidScope_ReturnsValidKernel()
    {
        OutputHelper.WriteLine($"Starting test: GetKernelByScope with scope '{TestScope1}'");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();

        using var wrapper = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);

        OutputHelper.WriteLine($"Retrieved kernel details:");
        OutputHelper.WriteLine($"- Scope: {string.Join(", ", wrapper.Scopes)}");
        OutputHelper.WriteLine($"- Kernel Instance: {wrapper.Kernel.GetHashCode()}");

        Assert.NotNull(wrapper);
        Assert.NotNull(wrapper.Kernel);
        Assert.Contains(TestScope1, wrapper.Scopes);

        OutputHelper.WriteLine("Successfully verified kernel properties");
    }

    [Fact]
    public async Task GetKernelByScope_WithInvalidScope_ThrowsInvalidOperationException()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByScope with invalid scope");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        const string invalidScope = "non-existent-scope";

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await kernelPoolManager.GetKernelByScopeAsync(invalidScope));

        OutputHelper.WriteLine($"Exception details: {exception.Message}");
        Assert.Contains($"No configuration found for scope {invalidScope}", exception.Message);
    }

    [Fact]
    public async Task GetKernelByScope_WhenRequestingTwo_AlternateBetweenPools()
    {
        // Arrange
        OutputHelper.WriteLine("Starting test: GetKernelByScope alternates between pools");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();

        // Act
        using var wrapper1 = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);
        OutputHelper.WriteLine($"First kernel obtained:");
        OutputHelper.WriteLine($"- Pool Unique Name: {wrapper1.UniqueName}");
        OutputHelper.WriteLine($"- Instance Hash: {wrapper1.Kernel.GetHashCode()}");

        using var wrapper2 = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);
        OutputHelper.WriteLine($"Second kernel obtained:");
        OutputHelper.WriteLine($"- Pool Unique Name: {wrapper2.UniqueName}");
        OutputHelper.WriteLine($"- Instance Hash: {wrapper2.Kernel.GetHashCode()}");

        // Assert
        Assert.NotNull(wrapper1.Kernel);
        Assert.NotNull(wrapper2.Kernel);
        Assert.NotEqual(wrapper1.Kernel.GetHashCode(), wrapper2.Kernel.GetHashCode());

        Assert.Contains(TestScope1, wrapper1.Scopes);
        Assert.Contains(TestScope1, wrapper2.Scopes);

        // Verify they're from different pools by checking their scopes
        Assert.NotEqual(wrapper1.UniqueName, wrapper2.UniqueName);

        OutputHelper.WriteLine($"Verified kernels came from different pools - Pool 1: {wrapper1.UniqueName}, Pool 2: {wrapper2.UniqueName}");
    }

    [Fact]
    public async Task GetKernelByScope_WhenAllPoolsExhausted_WaitsForTimeoutThenThrows()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByScope with all pools exhausted");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var kernels = new List<KernelWrapper>();

        try
        {
            // Exhaust all pools
            for (int i = 0; i < TotalPoolSize; i++)
            {
                var wrapper = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);
                kernels.Add(wrapper);
                OutputHelper.WriteLine($"Obtained kernel {i + 1}: Hash {wrapper.Kernel.GetHashCode()}");
            }

            var startTime = DateTime.UtcNow;
            OutputHelper.WriteLine($"All pools exhausted, attempting to get another kernel at {startTime:HH:mm:ss.fff}");

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await kernelPoolManager.GetKernelByScopeAsync(TestScope1));

            var duration = DateTime.UtcNow - startTime;
            OutputHelper.WriteLine($"Request failed after {duration.TotalSeconds:F2} seconds");
        }
        finally
        {
            foreach (var kernel in kernels)
            {
                kernel.Dispose();
            }
        }
    }

    [Fact]
    public async Task GetKernelByScope_WithMultipleRequests_DistributesLoad()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByScope load distribution");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var poolUsage = new Dictionary<string, int>();

        for (int i = 0; i < 10; i++)
        {
            using var wrapper = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);
            if (!poolUsage.ContainsKey(wrapper.UniqueName))
            {
                poolUsage[wrapper.UniqueName] = 0;
            }
            poolUsage[wrapper.UniqueName]++;

            OutputHelper.WriteLine($"Request {i + 1} used pool: {wrapper.UniqueName}");
        }

        foreach (var usage in poolUsage)
        {
            OutputHelper.WriteLine($"Pool {usage.Key} was used {usage.Value} times");
        }

        Assert.True(poolUsage.Count > 1, "Load should be distributed across multiple pools");
    }

    [Fact]
    public async Task GetKernelByScope_WithConcurrentRequests_HandlesThemCorrectly()
    {
        OutputHelper.WriteLine($"Starting test: GetKernelByScope with concurrent requests exceeding pool size");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var tasks = new List<Task<KernelWrapper>>();
        const int extraRequests = 3; // Number of requests exceeding pool size
        var concurrentRequests = TotalPoolSize + extraRequests;
        var successfulRequests = new List<KernelWrapper>();
        var failedRequests = 0;

        OutputHelper.WriteLine($"Total pool size: {TotalPoolSize}");
        OutputHelper.WriteLine($"Extra requests: {extraRequests}");
        OutputHelper.WriteLine($"Total concurrent requests: {concurrentRequests}");

        try
        {
            // Create multiple concurrent requests
            for (int i = 0; i < concurrentRequests; i++)
            {
                var requestId = i; // Capture for logging
                var task = Task.Run(async () =>
                {
                    try
                    {
                        OutputHelper.WriteLine($"Starting request {requestId + 1}/{concurrentRequests}");
                        return await kernelPoolManager.GetKernelByScopeAsync(TestScope1);
                    }
                    catch (InvalidOperationException ex)
                    {
                        OutputHelper.WriteLine($"Request {requestId + 1} failed as expected: {ex.Message}");
                        Interlocked.Increment(ref failedRequests);
                        throw;
                    }
                });
                tasks.Add(task);
            }

            // Wait for all tasks and collect results
            var results = await Task.WhenAll(
                tasks.Select(async t =>
                {
                    try
                    {
                        return await t;
                    }
                    catch
                    {
                        return null;
                    }
                })
            );

            // Count successful requests
            successfulRequests.AddRange(results.Where(r => r != null)!);
            var uniqueKernels = successfulRequests.Select(w => w.Kernel.GetHashCode()).Distinct().Count();

            OutputHelper.WriteLine($"\nRequest Summary:");
            OutputHelper.WriteLine($"- Total requests: {concurrentRequests}");
            OutputHelper.WriteLine($"- Successful requests: {successfulRequests.Count}");
            OutputHelper.WriteLine($"- Failed requests: {failedRequests}");
            OutputHelper.WriteLine($"- Unique kernels obtained: {uniqueKernels}");

            // Verify results
            Assert.Equal(TotalPoolSize, successfulRequests.Count);
            Assert.Equal(extraRequests, failedRequests);
            Assert.True(uniqueKernels <= TotalPoolSize,
                $"Got {uniqueKernels} unique kernels, expected maximum of {TotalPoolSize}");
        }
        finally
        {
            // Dispose successful kernels
            foreach (var kernel in successfulRequests)
            {
                kernel.Dispose();
            }
            OutputHelper.WriteLine("Cleaned up all successful kernel requests");
        }
    }

    [Fact]
    public async Task GetKernelByScope_WhenDisposedAndReacquired_ReusesKernels()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByScope kernel reuse");
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var seenKernels = new HashSet<int>();
        const int configCount = 2; // Number of OpenAI configurations

        for (int i = 0; i < TotalPoolSize; i++)
        {
            using var wrapper = await kernelPoolManager.GetKernelByScopeAsync(TestScope1);
            var kernelHash = wrapper.Kernel.GetHashCode();
            OutputHelper.WriteLine($"Request {i + 1} got kernel with hash: {kernelHash} from {wrapper.UniqueName}");
            seenKernels.Add(kernelHash);
        }

        OutputHelper.WriteLine($"Total unique kernels created: {seenKernels.Count}");
        Assert.Equal(TotalPoolSize / configCount, seenKernels.Count);
    }
}