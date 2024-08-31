namespace SemanticKernelPooling.Connectors.MistralAI;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Represents the configuration settings required for the Mistral AI service provider.
/// </summary>
/// <remarks>
/// This record inherits from <see cref="AIServiceProviderConfiguration"/> and includes specific settings 
/// required to connect to and use Mistral AI services, such as the model ID, API key, optional endpoint, and service ID.
/// </remarks>
public record MistralAIConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    /// <summary>
    /// Gets or initializes the ID of the model to use with the Mistral AI service.
    /// </summary>
    /// <remarks>
    /// This property specifies the unique identifier for the model that will be used in requests to the Mistral AI service.
    /// </remarks>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets or initializes the endpoint URL for the Mistral AI service.
    /// </summary>
    /// <remarks>
    /// This property specifies the base URL endpoint for the Mistral AI API. If not specified, a default endpoint will be used.
    /// </remarks>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Gets or initializes the API key required to authenticate requests to the Mistral AI service.
    /// </summary>
    /// <remarks>
    /// This property is required to authenticate with the Mistral AI service and authorize API requests.
    /// </remarks>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets or initializes the service ID for targeting specific services within the Mistral AI provider.
    /// </summary>
    /// <remarks>
    /// This property allows for specifying a particular service or endpoint within the Mistral AI infrastructure.
    /// </remarks>
    public required string ServiceId { get; init; }
}