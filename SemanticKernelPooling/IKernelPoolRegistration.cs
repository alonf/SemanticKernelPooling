using Microsoft.SemanticKernel;

namespace SemanticKernelPooling;

/// <summary>
/// Registration interface for kernel pools that support customization of the kernel building process.
/// </summary>
/// <typeparam name="TServiceProviderConfiguration"></typeparam>
public interface IKernelPoolRegistration<out TServiceProviderConfiguration> : IKernelPool where TServiceProviderConfiguration : AIServiceProviderConfiguration
{
    /// <summary>
    /// Registers an action to be executed before a kernel is created. This action allows for 
    /// customization of the kernel building process.
    /// </summary>
    /// <param name="action">The action to be executed before kernel creation, which takes an <see cref="IKernelBuilder"/></param>
    /// <exception cref="InvalidOperationException">Thrown if kernels are already present in the pool.</exception>
    void RegisterForPreKernelCreation(Action<IKernelBuilder, TServiceProviderConfiguration, KernelBuilderOptions> action);

    /// <summary>
    /// Registers an action to be executed before a kernel is created. This action allows for
    /// customization of the kernel building process. The action is registered with a usage key, enabling different type of kernel providers
    /// to share the same configuration.
    /// </summary>
    /// <param name="scope">The scope that enables filtering various kernel types to use the same configuration</param>
    /// <param name="action">The configuration function</param>
    void RegisterForPreKernelCreation(string scope, Action<IKernelBuilder, TServiceProviderConfiguration, KernelBuilderOptions, IReadOnlyList<string>> action);

    /// <summary>
    /// Registers an action to be executed after a kernel is created. This action allows for 
    /// additional initialization or configuration of the kernel after it has been built.
    /// </summary>
    /// <param name="action">The action to be executed after kernel creation, which takes a <see cref="Kernel"/></param>
    /// <exception cref="InvalidOperationException">Thrown if kernels are already present in the pool.</exception>
    void RegisterForAfterKernelCreation(Action<Kernel, TServiceProviderConfiguration> action);

    /// <summary>
    /// Registers an action to be executed after a kernel is created. This action allows for
    /// customization of the kernel building process. The action is registered with a usage key, enabling different type of kernel providers
    /// to share the same configuration.
    /// </summary>
    /// <param name="scope">The scope that enables filtering various kernel types to use the same configuration</param>
    /// <param name="action">The configuration function</param>

    void RegisterForAfterKernelCreation(string scope, Action<Kernel, TServiceProviderConfiguration, IReadOnlyList<string>> action);
}