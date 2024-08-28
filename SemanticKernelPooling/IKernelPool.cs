using Microsoft.SemanticKernel;

namespace SemanticKernelPooling;

public interface IKernelPool
{
    /// <summary>
    /// Initializes the kernel pool with a set number of kernels based on the configurations provided.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Asynchronously retrieves a kernel from the pool. Waits up to a maximum time specified 
    /// if no kernels are currently available.
    /// </summary>
    /// <returns>A <see cref="KernelWrapper"/> containing the kernel and its associated metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no kernels are available after waiting for the specified time.</exception>
    Task<KernelWrapper> GetKernelAsync();

    /// <summary>
    /// Registers an action to be executed before a kernel is created. This action allows for 
    /// customization of the kernel building process.
    /// </summary>
    /// <param name="action">The action to be executed before kernel creation, which takes an <see cref="IKernelBuilder"/> 
    /// and <see cref="AzureOpenAIConfiguration"/> as parameters.</param>
    /// <exception cref="InvalidOperationException">Thrown if kernels are already present in the pool.</exception>
    void RegisterForPreKernelCreation(Action<IKernelBuilder, AzureOpenAIConfiguration> action);

    /// <summary>
    /// Registers an action to be executed after a kernel is created. This action allows for 
    /// additional initialization or configuration of the kernel after it has been built.
    /// </summary>
    /// <param name="action">The action to be executed after kernel creation, which takes a <see cref="Kernel"/> 
    /// and <see cref="AzureOpenAIConfiguration"/> as parameters.</param>
    /// <exception cref="InvalidOperationException">Thrown if kernels are already present in the pool.</exception>
    void RegisterForAfterKernelCreation(Action<Kernel, AzureOpenAIConfiguration> action);

    /// <summary>
    /// Returns a kernel to the pool, making it available for reuse.
    /// </summary>
    /// <param name="kernel">The kernel to return to the pool.</param>
    void ReturnKernel(Kernel kernel);
}