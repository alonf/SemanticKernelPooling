namespace SemanticKernelPooling;

// ReSharper disable UnusedAutoPropertyAccessor.Global

/// <summary>
/// Represents the base configuration for an AI service provider.
/// </summary>
/// <remarks>
/// This abstract record provides common configuration settings for different types of AI service providers,
/// such as OpenAI, AzureOpenAI, and other custom providers. Derived records must specify their unique configuration 
/// properties while inheriting these common settings.
/// </remarks>
public abstract record AIServiceProviderConfiguration
{
    /// <summary>
    /// Gets or initializes a unique name for the AI service configuration.
    /// </summary>
    /// <remarks>
    /// This name is used to uniquely identify the service configuration instance, 
    /// especially when multiple configurations of the same type are used.
    /// </remarks>
    public required string UniqueName { get; init; }

    /// <summary>
    /// Gets or initializes the type of the AI service provider.
    /// </summary>
    /// <remarks>
    /// Specifies the type of AI service provider, such as AzureOpenAI, OpenAI, MistralAI, etc.
    /// This helps in selecting the appropriate configuration and service provider during runtime.
    /// </remarks>
    public required AIServiceProviderType ServiceType { get; init; }

    /// <summary>
    /// Gets or initializes the number of instances for this AI service configuration.
    /// </summary>
    /// <remarks>
    /// Defines the number of kernel instances that can be created and managed for this service configuration.
    /// </remarks>
    public int InstanceCount { get; init; }

    /// <summary>
    /// Gets or sets the list of scopes for this configuration.
    /// </summary>
    /// <remarks>
    /// A scope is used to group kernel providers that share common characteristics or usage scenarios.
    /// Multiple scopes can be assigned to a single configuration to support complex grouping and selection logic.
    /// </remarks>
    public List<string> Scopes { get; set; } = new List<string>();

    /// <summary>
    /// Gets or initializes the deployment text embedding used for text processing or embedding operations.
    /// </summary>
    /// <remarks>
    /// This setting is required to specify the text embedding deployment used by the AI service.
    /// </remarks>
    public required string DeploymentTextEmbedding { get; init; }

    /// <summary>
    /// Gets or sets the maximum wait time in seconds for a kernel to be available from the pool.
    /// </summary>
    /// <remarks>
    /// Defines the maximum duration to wait for a kernel to be available before timing out.
    /// </remarks>
    public int MaxWaitForKernelInSeconds { get; set; }
}