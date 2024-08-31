using Microsoft.SemanticKernel;

namespace SemanticKernelPooling;

public interface IKernelPool
{
    /// <summary>
    /// Asynchronously retrieves a kernel from the pool. Waits up to a maximum time specified 
    /// if no kernels are currently available.
    /// </summary>
    /// <returns>A <see cref="KernelWrapper"/> containing the kernel and its associated metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no kernels are available after waiting for the specified time.</exception>
    Task<KernelWrapper> GetKernelAsync();

    /// <summary>
    /// Returns a kernel to the pool, making it available for reuse.
    /// </summary>
    /// <param name="kernel">The kernel to return to the pool.</param>
    void ReturnKernel(Kernel kernel);

    /// <summary>
    /// The list of scopes that this kernel pool supports.
    /// </summary>
    IReadOnlyList<string> Scopes { get; }

    /// <summary>
    /// The type of AI service provider that this kernel pool supports.
    /// </summary>
    AIServiceProviderType ServiceProviderType { get; }
}