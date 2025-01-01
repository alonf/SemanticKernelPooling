using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit;

[Collection("KernelPoolManagerTests")]
public class KernelPoolManagerGetByServiceTypeTests
{
    private IServiceProvider _serviceProvider => _fixture.ServiceProvider;
    private readonly SemanticKernelPoolTestFixture _fixture;
    private ITestOutputHelper OutputHelper => _fixture.TestOutputHelper;

    public KernelPoolManagerGetByServiceTypeTests(SemanticKernelPoolTestFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _fixture.ConfigureLogging(outputHelper);
        _fixture.InitializeServices();
        OutputHelper.WriteLine("Initializing KernelPoolManagerGetByServiceTypeTests");
    }
}