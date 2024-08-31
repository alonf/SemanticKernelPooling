namespace SemanticKernelPooling;

/// <summary>
/// Interface for retrieving kernels from a pool for different AI service providers.
/// </summary>
public interface IKernelPoolManager
{
    /// <summary>
    /// Asynchronously retrieves a kernel wrapper from the pool for a specified AI service provider type.
    /// </summary>
    /// <param name="aiServiceProviderType">The type of the AI service provider to get the kernel for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="KernelWrapper"/> 
    /// that provides access to the kernel instance and its associated metadata.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no configuration is found for the specified AI service provider type 
    /// or if the kernel pool cannot be created.
    /// </exception>
    Task<KernelWrapper> GetKernelAsync(AIServiceProviderType aiServiceProviderType);
}