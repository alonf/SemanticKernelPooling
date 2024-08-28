using Microsoft.SemanticKernel;
using System.Dynamic;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPooling;

public class KernelWrapper(Kernel kernel, IKernelPool pool, ILogger logger) : DynamicObject, IDisposable
{
    private readonly DateTimeOffset _creationTime = DateTimeOffset.UtcNow;
    private bool _disposed;

    public Kernel Kernel => kernel;

    public void Dispose()
    {
        if (_disposed)
            return;

        pool.ReturnKernel(kernel);
        _disposed = true;
        logger.LogInformation("Kernel usage time: {usageTime}",
            DateTimeOffset.UtcNow - _creationTime);
    }
}