using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit;

public class TestOutputHelper : ITestOutputHelper
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    public void WriteLine(string message)
    {
        try
        {
            _testOutputHelper.WriteLine(message);
        }
        catch
        {
            Console.WriteLine(message);
        }
    }

    public void WriteLine(string format, params object[] args)
    {
        try
        {
            _testOutputHelper.WriteLine(format, args);
        }
        catch
        {
            Console.WriteLine(format, args);
        }
    }
}