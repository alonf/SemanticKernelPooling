using Microsoft.SemanticKernel;
using System.Dynamic;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPooling;

/// <summary>
/// Represents a wrapper around a <see cref="Kernel"/> instance, providing additional metadata
/// and management functionality, such as disposing and returning the kernel to the pool.
/// Initializes a new instance of the <see cref="KernelWrapper"/> class.
/// </summary>
/// <param name="kernel">The <see cref="Kernel"/> instance to wrap.</param>
/// <param name="pool">The kernel pool from which the kernel was retrieved.</param>
/// <param name="logger">The logger used for logging kernel usage information.</param>
public class KernelWrapper(Kernel kernel, IKernelPool pool, ILogger logger) : DynamicObject, IDisposable
{
    private readonly DateTimeOffset _creationTime = DateTimeOffset.UtcNow;
    private bool _disposed;

    /// <summary>
    /// Gets the wrapped <see cref="Kernel"/> instance.
    /// </summary>
    public Kernel Kernel => kernel;

    /// <summary>
    /// Disposes of the <see cref="KernelWrapper"/> instance, returning the wrapped kernel to its pool.
    /// </summary>
    /// <remarks>
    /// This method ensures that the kernel is returned to its pool and logs the usage time of the kernel.
    /// </remarks>
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