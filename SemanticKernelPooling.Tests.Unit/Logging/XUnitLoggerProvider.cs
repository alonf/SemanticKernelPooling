using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit.Logging;

public class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper;

    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
        => new XUnitLogger(_testOutputHelper, categoryName);

    public void Dispose(){ }
}