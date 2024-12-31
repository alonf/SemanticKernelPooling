using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SemanticKernelPooling.Tests.Unit.Logging
{
    internal class XUnitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;

        public XUnitLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                _testOutputHelper.WriteLine($"{_categoryName} [{eventId}] {formatter(state, exception!)}");

                if (exception != null)
                    _testOutputHelper.WriteLine(exception.ToString());
            }
            catch (InvalidOperationException)
            {
                //no active test, ignore!
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();
            public void Dispose()
            { }
        }
    }
}
