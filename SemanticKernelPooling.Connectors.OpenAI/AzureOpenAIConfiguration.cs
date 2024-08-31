namespace SemanticKernelPooling.Connectors.OpenAI;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Represents the configuration settings required for the Azure OpenAI service provider.
/// </summary>
/// <remarks>
/// This record inherits from <see cref="AIServiceProviderConfiguration"/> and includes specific settings 
/// required to connect to and use Azure OpenAI services, such as the deployment name, API key, model ID, endpoint, and service ID.
/// </remarks>
public record AzureOpenAIConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    /// <summary>
    /// Gets or initializes the deployment name for the Azure OpenAI service.
    /// </summary>
    /// <remarks>
    /// This property specifies the deployment name within the Azure OpenAI service, which corresponds to a specific model deployment.
    /// </remarks>
    public required string DeploymentName { get; init; }

    /// <summary>
    /// Gets or initializes the API key required to authenticate requests to the Azure OpenAI service.
    /// </summary>
    /// <remarks>
    /// This property is required to authenticate with the Azure OpenAI service and authorize API requests.
    /// </remarks>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets or initializes the model ID for the Azure OpenAI service.
    /// </summary>
    /// <remarks>
    /// This property specifies the unique identifier for the model that will be used in requests to the Azure OpenAI service.
    /// </remarks>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets or initializes the endpoint URL for the Azure OpenAI service.
    /// </summary>
    /// <remarks>
    /// This property specifies the base URL endpoint for the Azure OpenAI API, typically including the protocol (e.g., "https://").
    /// </remarks>
    public required string Endpoint { get; init; }

    /// <summary>
    /// Gets or initializes the service ID for targeting specific services within the Azure OpenAI provider.
    /// </summary>
    /// <remarks>
    /// This property allows for specifying a particular service or endpoint within the Azure OpenAI infrastructure.
    /// </remarks>
    public required string ServiceId { get; init; }
}