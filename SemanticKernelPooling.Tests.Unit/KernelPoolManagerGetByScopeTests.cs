using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit;

[Collection("KernelPoolManagerTests")]
public class KernelPoolManagerGetByScopeTests
{
    private IServiceProvider _serviceProvider => _fixture.ServiceProvider;
    private readonly SemanticKernelPoolTestFixture _fixture;
    private ITestOutputHelper OutputHelper => _fixture.TestOutputHelper;

    public KernelPoolManagerGetByScopeTests(SemanticKernelPoolTestFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _fixture.ConfigureLogging(outputHelper);
        _fixture.InitializeServices();
        OutputHelper.WriteLine("Initializing KernelPoolManagerGetByScopeTests");
    }
}