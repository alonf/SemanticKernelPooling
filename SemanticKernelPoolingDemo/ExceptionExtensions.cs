using System.Net;

namespace SemanticKernelPoolingDemo;

public static class ExceptionExtensions
{
    /// <summary>
    /// Flattens an exception and all of its inner exceptions, including aggregate exceptions, into a single list.
    /// </summary>
    /// <param name="exception">The root exception to flatten.</param>
    /// <returns>A list of all exceptions, including nested and aggregate exceptions.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static List<Exception> Flatten(this Exception exception)
    {
        var result = new List<Exception>();
        var exceptions = new Stack<Exception>();

        exceptions.Push(exception);

        while (exceptions.Any())
        {
            var current = exceptions.Pop();
            result.Add(current);

            if (current is AggregateException aggEx)
            {
                foreach (var inner in aggEx.InnerExceptions)
                {
                    exceptions.Push(inner);
                }
            }
            else if (current.InnerException != null)
            {
                exceptions.Push(current.InnerException);
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether an exception is a transient error that can be retried.
    /// </summary>
    /// <param name="exception">The exception</param>
    /// <returns>true if the exception is retry-able</returns>
    public static bool IsTransientError(this Exception exception)
    {
        var allExceptions = exception.Flatten();

        var isTransient = allExceptions.Any(ex => ex switch
        {
            // Check for HTTP request exceptions with status code 429 (Too Many Requests)
            HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests } => true,

            // Check for timeout exceptions
            TaskCanceledException => true,
            TimeoutException => true,

            // Check for Azure.RequestFailedException with transient error status codes
            Azure.RequestFailedException { Status: 408 or 429 or 500 or 502 or 503 or 504 } => true,
            _ => false,
        });

        return isTransient;
    }
}