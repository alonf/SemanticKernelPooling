using Microsoft.SemanticKernel;

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

    /// <summary>
    /// Asynchronously retrieves a kernel wrapper from the pool for a service provider with a predefined scope.
    /// </summary>
    /// <param name="scope">The scope name to filter the kernel pool</param>
    /// <returns>A kernel wrapper</returns>
    Task<KernelWrapper> GetKernelAsync(string scope);

    /// <summary>
    /// Registers an action to be executed before a kernel is created. This action allows for 
    /// customization of the kernel building process.
    /// </summary>
    /// <param name="action">The action to be executed before kernel creation, which takes an <see cref="IKernelBuilder"/></param>
    /// <exception cref="InvalidOperationException">Thrown if kernels are already present in the pool.</exception>
    void RegisterForPreKernelCreation<TServiceProviderConfiguration>(
        Action<IKernelBuilder, TServiceProviderConfiguration, KernelBuilderOptions> action)
        where TServiceProviderConfiguration : AIServiceProviderConfiguration;

    /// <summary>
    /// Registers an action to be executed before a kernel is created. This action allows for 
    /// customization of the kernel building process. The action is registered with a scope, enabling different type of kernel providers
    /// to share the same configuration.
    /// </summary>
    /// <param name="scope">The scope of the kernel that need to be configured</param>
    /// <param name="action">the configuration function</param>
    void RegisterForPreKernelCreation(string scope, Action<IKernelBuilder, AIServiceProviderConfiguration, KernelBuilderOptions, IReadOnlyList<string>> action);

    /// <summary>
    /// Registers an action to be executed after a kernel is created. This action allows for 
    /// additional initialization or configuration of the kernel after it has been built.
    /// </summary>
    /// <param name="action">The action to be executed after kernel creation, which takes a <see cref="Kernel"/></param>
    /// <exception cref="InvalidOperationException">Thrown if kernels are already present in the pool.</exception>
    void RegisterForAfterKernelCreation<TServiceProviderConfiguration>(
        Action<Kernel, TServiceProviderConfiguration> action)
        where TServiceProviderConfiguration : AIServiceProviderConfiguration;

    /// <summary>
    /// Registers an action to be executed after a kernel is created. This action allows for
    /// additional initialization or configuration of the kernel after it has been built.
    /// The action is registered with a scope, enabling different type of kernel providers
    /// to share the same configuration.
    /// </summary>
    /// <param name="scope">The scope that need to be configured</param>
    /// <param name="action">The configuration function</param>
    void RegisterForAfterKernelCreation(string scope, Action<Kernel, AIServiceProviderConfiguration, IReadOnlyList<string>> action);
}