using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SemanticKernelPooling.Tests.Mocks;
using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit;

[Collection("KernelPoolManagerTests")]
public class KernelPoolManagerGetByNameTests
{
    private IServiceProvider _serviceProvider => _fixture.ServiceProvider;
    private readonly SemanticKernelPoolTestFixture _fixture;
    private ITestOutputHelper OutputHelper => _fixture.TestOutputHelper;
    private const string ValidUniqueName = "MockOpenAI";
    private int PoolSize => _fixture.PoolSize;
    private string TestScope => _fixture.TestScope;

    public KernelPoolManagerGetByNameTests(SemanticKernelPoolTestFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _fixture.ConfigureLogging(outputHelper);
        _fixture.InitializeServices();
        OutputHelper.WriteLine("Initializing KernelPoolManagerGetByNameTests");
    }

    [Fact]
    public async Task GetKernelByName_WithValidName_ReturnsKernelWithCorrectServiceType()
    {
        OutputHelper.WriteLine($"Starting test: GetKernelByName with valid name '{ValidUniqueName}'");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();

        using var kernelWrapper = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
        OutputHelper.WriteLine($"Retrieved kernel details:");
        OutputHelper.WriteLine($"- Service Type: {kernelWrapper.ServiceProviderType}");
        OutputHelper.WriteLine($"- Kernel Instance: {kernelWrapper.Kernel.GetHashCode()}");

        Assert.NotNull(kernelWrapper);
        Assert.NotNull(kernelWrapper.Kernel);
        Assert.Equal(AIServiceProviderType.OpenAI, kernelWrapper.ServiceProviderType);
        OutputHelper.WriteLine($"Successfully verified kernel properties");
    }

    [Fact]
    public async Task GetKernelByName_WhenHoldingTwoKernels_BothAreDifferentInstances()
    {
        OutputHelper.WriteLine($"Starting test: GetKernelByName requesting two kernels simultaneously");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();

        using var wrapper1 = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
        OutputHelper.WriteLine($"First kernel obtained:");
        OutputHelper.WriteLine($"- Instance Hash: {wrapper1.Kernel.GetHashCode()}");

        using var wrapper2 = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
        OutputHelper.WriteLine($"Second kernel obtained:");
        OutputHelper.WriteLine($"- Instance Hash: {wrapper2.Kernel.GetHashCode()}");

        Assert.NotSame(wrapper1.Kernel, wrapper2.Kernel);
        OutputHelper.WriteLine("Verified both kernels are different instances");
    }

    [Fact]
    public async Task GetKernelByName_WhenDisposingAndReacquiring_ReusesKernel()
    {
        OutputHelper.WriteLine($"Starting test: GetKernelByName with disposal and reacquisition");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();

        Kernel firstKernel;
        using (var wrapper1 = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName))
        {
            firstKernel = wrapper1.Kernel;
            OutputHelper.WriteLine($"First kernel obtained:");
            OutputHelper.WriteLine($"- Instance Hash: {firstKernel.GetHashCode()}");
        }
        OutputHelper.WriteLine("First kernel disposed and returned to pool");

        using var wrapper2 = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
        OutputHelper.WriteLine($"Second kernel obtained:");
        OutputHelper.WriteLine($"- Instance Hash: {wrapper2.Kernel.GetHashCode()}");

        Assert.Same(firstKernel, wrapper2.Kernel);
        OutputHelper.WriteLine("Verified second kernel is same instance as first");
    }

    [Fact]
    public async Task GetKernelByName_WithInvalidName_ThrowsInvalidOperationException()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByName with invalid name");

        // Arrange
        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        const string invalidName = "NonExistentKernel";
        OutputHelper.WriteLine($"Attempting to get kernel with invalid name: {invalidName}");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await kernelPoolManager.GetKernelByNameAsync(invalidName));

        OutputHelper.WriteLine($"Exception details:");
        OutputHelper.WriteLine($"- Type: {exception.GetType().Name}");
        OutputHelper.WriteLine($"- Message: {exception.Message}");

        // Verify the exception type and a more generic error message
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains($"No configuration found for kernel pool name {invalidName}", exception.Message);

        OutputHelper.WriteLine("Successfully verified exception type and message");
    }

    [Fact]
    public async Task GetKernelByName_WhenPoolExhausted_WaitsForTimeoutThenThrows()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByName with pool exhaustion and timeout");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        const int expectedTimeoutSeconds = 1; // From appsettings
        var timeoutBuffer = TimeSpan.FromSeconds(0.5); // Add buffer for system overhead

        try
        {
            // Hold both available kernels
            using var wrapper1 = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
            OutputHelper.WriteLine($"First kernel obtained: Hash {wrapper1.Kernel.GetHashCode()}");

            using var wrapper2 = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
            OutputHelper.WriteLine($"Second kernel obtained: Hash {wrapper2.Kernel.GetHashCode()}");
            OutputHelper.WriteLine($"Pool is now exhausted (2/2 kernels in use)");

            // Try to get a third kernel - should wait then throw
            var startTime = DateTime.UtcNow;
            OutputHelper.WriteLine($"Attempting to get third kernel at {startTime:HH:mm:ss.fff}");
            OutputHelper.WriteLine($"Should wait for {expectedTimeoutSeconds} seconds before throwing");

            // Create a task that attempts to get a kernel
            var getKernelTask = kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);

            // Wait for either the timeout period or task completion
            var completedTask = await Task.WhenAny(
                getKernelTask,
                Task.Delay(TimeSpan.FromSeconds(expectedTimeoutSeconds) + timeoutBuffer)
            );

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // If the kernel task completed, it should have thrown an exception
            if (completedTask == getKernelTask)
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await getKernelTask);

                OutputHelper.WriteLine($"Exception thrown at {endTime:HH:mm:ss.fff}");
                OutputHelper.WriteLine($"Actual wait duration: {duration.TotalSeconds:F3} seconds");
                OutputHelper.WriteLine($"Exception message: {exception.Message}");

                // Verify the timeout duration
                Assert.True(duration.TotalSeconds >= expectedTimeoutSeconds,
                    $"Timeout occurred after {duration.TotalSeconds:F3} seconds, expected at least {expectedTimeoutSeconds} seconds");

                Assert.Contains("No available kernels", exception.Message);
            }
            else
            {
                // If we reached here without an exception, the test failed
                Assert.Fail($"Test timed out after {duration.TotalSeconds:F3} seconds without throwing expected exception");
            }
        }
        finally
        {
            OutputHelper.WriteLine("Test completed");
        }
    }

    [Fact]
    public async Task GetKernelByName_WhenKernelDisposedMultipleTimes_HandlesGracefully()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByName with multiple dispose calls");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();

        var wrapper = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
        var kernelHash = wrapper.Kernel.GetHashCode();
        OutputHelper.WriteLine($"Obtained kernel: Hash {kernelHash}");

        // First dispose - should work normally
        wrapper.Dispose();
        OutputHelper.WriteLine("First dispose completed");

        // Second dispose - should not throw
        wrapper.Dispose();
        OutputHelper.WriteLine("Second dispose completed without error");

        // Verify we can get the same kernel back
        using var newWrapper = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
        OutputHelper.WriteLine($"Retrieved new kernel: Hash {newWrapper.Kernel.GetHashCode()}");

        Assert.Equal(kernelHash, newWrapper.Kernel.GetHashCode());
    }

    [Fact]
    public async Task GetKernelByName_UnderLoad_MaintainsPoolSize()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByName under load conditions");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var kernelHashes = new HashSet<int>();
        const int numberOfIterations = 10;

        for (int i = 0; i < numberOfIterations; i++)
        {
            OutputHelper.WriteLine($"Iteration {i + 1} of {numberOfIterations}");
            var wrappers = new List<KernelWrapper>();

            // Request maximum pool size kernels
            for (int j = 0; j < PoolSize; j++)
            {
                var wrapper = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
                wrappers.Add(wrapper);
                kernelHashes.Add(wrapper.Kernel.GetHashCode());
                OutputHelper.WriteLine($"Got kernel: Hash {wrapper.Kernel.GetHashCode()}");
            }

            // Dispose all kernels
            foreach (var wrapper in wrappers)
            {
                wrapper.Dispose();
            }
            OutputHelper.WriteLine("All kernels returned to pool");
        }

        // Verify we never created more unique kernels than the pool size
        OutputHelper.WriteLine($"Total unique kernels created: {kernelHashes.Count}");
        Assert.Equal(PoolSize, kernelHashes.Count);
    }

    [Fact(Skip = "Need to add possibility to cancel kernel request")]
    public async Task GetKernelByName_WithCancellation_StopsWaiting()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByName with cancellation");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        var cts = new CancellationTokenSource();

        // Hold all available kernels
        var heldKernels = new List<KernelWrapper>();
        for (int i = 0; i < PoolSize; i++)
        {
            var wrapper = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
            heldKernels.Add(wrapper);
            OutputHelper.WriteLine($"Holding kernel {i + 1}: Hash {wrapper.Kernel.GetHashCode()}");
        }

        try
        {
            // Try to get another kernel with cancellation
            var getKernelTask = Task.Run(async () =>
            {
                try
                {
                    return await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
                }
                catch (OperationCanceledException)
                {
                    OutputHelper.WriteLine("Task was cancelled as expected");
                    throw;
                }
            }, cts.Token);

            // Cancel after a short delay
            await Task.Delay(100);
            cts.Cancel();
            OutputHelper.WriteLine("Cancellation requested");

            // Verify the operation was cancelled
            await Assert.ThrowsAsync<OperationCanceledException>(() => getKernelTask);
        }
        finally
        {
            // Cleanup
            foreach (var wrapper in heldKernels)
            {
                wrapper.Dispose();
            }
            cts.Dispose();
        }
    }

    [Fact]
    public async Task GetKernelByName_WhenDisposedInDifferentThread_HandlesCorrectly()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByName with disposal in different thread");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        KernelWrapper wrapper = null!;
        int kernelHash = 0;

        // Get kernel in main thread
        wrapper = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
        kernelHash = wrapper.Kernel.GetHashCode();
        OutputHelper.WriteLine($"Got kernel in main thread: Hash {kernelHash}");

        // Dispose in different thread
        await Task.Run(() =>
        {
            OutputHelper.WriteLine($"Disposing kernel in separate thread: Hash {kernelHash}");
            wrapper.Dispose();
        });

        // Verify we can get the same kernel back
        using var newWrapper = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
        OutputHelper.WriteLine($"Retrieved kernel back in main thread: Hash {newWrapper.Kernel.GetHashCode()}");
        Assert.Equal(kernelHash, newWrapper.Kernel.GetHashCode());
    }

    [Fact]
    public async Task GetKernelByName_WithRapidRequestAndDispose_HandlesRaceConditions()
    {
        OutputHelper.WriteLine("Starting test: GetKernelByName with rapid request/dispose cycles");

        var kernelPoolManager = _serviceProvider.GetRequiredService<IKernelPoolManager>();
        const int numberOfCycles = 20;
        var tasks = new List<Task>();
        var startTime = DateTime.UtcNow;

        for (int i = 0; i < numberOfCycles; i++)
        {
            var cycleIndex = i;
            var task = Task.Run(async () =>
            {
                OutputHelper.WriteLine($"Cycle {cycleIndex}: Requesting kernel at {(DateTime.UtcNow - startTime).TotalMilliseconds:F2}ms");

                using var wrapper = await kernelPoolManager.GetKernelByNameAsync(ValidUniqueName);
                var acquiredTime = DateTime.UtcNow;
                OutputHelper.WriteLine($"Cycle {cycleIndex}: Acquired kernel Hash {wrapper.Kernel.GetHashCode()} at {(acquiredTime - startTime).TotalMilliseconds:F2}ms");

                // Add small random delay to simulate some work
                await Task.Delay(Random.Shared.Next(10, 50));

                // Log before disposal
                var disposingTime = DateTime.UtcNow;
                OutputHelper.WriteLine($"Cycle {cycleIndex}: Disposing kernel Hash {wrapper.Kernel.GetHashCode()} at {(disposingTime - startTime).TotalMilliseconds:F2}ms");
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        OutputHelper.WriteLine($"All cycles completed after {(endTime - startTime).TotalMilliseconds:F2}ms");
    }
}