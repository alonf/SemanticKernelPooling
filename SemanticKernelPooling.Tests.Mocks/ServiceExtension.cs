using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPooling.Tests.Mocks;

/// <summary>
/// Provides extension methods for registering mock kernel pools with the service collection.
/// </summary>
/// <remarks>
/// This extension follows the same pattern as the production service extensions but provides
/// mock implementations for testing purposes.
/// </remarks>
public static class ServiceExtension
{
    /// <summary>
    /// Registers mock kernel pool implementations for testing purposes.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider.</param>
    /// <param name="responsesPath">Optional path to mock response files. Defaults to "MockResponses".</param>
    /// <returns>The service provider to enable method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method registers mock implementations for both OpenAI and Azure OpenAI service types,
    /// allowing tests to run without actual service dependencies.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// services.AddSingleton(mockConfiguration.Object);
    /// services.UseSemanticKernelPooling();
    /// serviceProvider.UseMockKernelPool("TestResponses");
    /// </code>
    /// </para>
    /// </remarks>
    public static IServiceProvider UseMockKernelPool(
        this IServiceProvider serviceProvider,
        string responsesPath = "MockResponses")
    {
        var registrar = serviceProvider.GetRequiredService<IKernelPoolFactoryRegistrar>();

        // Register factory for both mock providers
        var factory = (AIServiceProviderConfiguration config, ILoggerFactory loggerFactory) =>
            new MockKernelPool((MockAIConfiguration)config, loggerFactory, responsesPath);

        registrar.RegisterKernelPoolFactory(AIServiceProviderType.OpenAI, factory);
        registrar.RegisterKernelPoolFactory(AIServiceProviderType.AzureOpenAI, factory);

        // Register configuration readers
        var configReader = (IConfigurationSection section) =>
            section.Get<MockAIConfiguration>() ??
            throw new InvalidOperationException("Mock configuration not found.");

        registrar.RegisterConfigurationReader(AIServiceProviderType.OpenAI, configReader);
        registrar.RegisterConfigurationReader(AIServiceProviderType.AzureOpenAI, configReader);

        return serviceProvider;
    }
}