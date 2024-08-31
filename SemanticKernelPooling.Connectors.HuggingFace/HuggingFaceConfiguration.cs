namespace SemanticKernelPooling.Connectors.HuggingFace;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Represents the configuration settings required for the HuggingFace AI service provider.
/// </summary>
/// <remarks>
/// This record inherits from <see cref="AIServiceProviderConfiguration"/> and adds specific settings 
/// required to connect to and use HuggingFace services, including the model name, API key, endpoint, and service ID.
/// </remarks>
public record HuggingFaceConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    /// <summary>
    /// Gets or initializes the name of the model to use with the HuggingFace service.
    /// </summary>
    /// <remarks>
    /// This property specifies the model identifier that will be used in requests to the HuggingFace API.
    /// </remarks>
    public required string Model { get; init; }

    /// <summary>
    /// Gets or initializes the API key required to authenticate requests to the HuggingFace service.
    /// </summary>
    /// <remarks>
    /// This property is required to authenticate with the HuggingFace service and authorize API requests.
    /// </remarks>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets or initializes the endpoint URL for the HuggingFace service.
    /// </summary>
    /// <remarks>
    /// This property specifies the base URL endpoint for the HuggingFace API, typically including the protocol (e.g., "https://").
    /// </remarks>
    public required string Endpoint { get; init; }

    /// <summary>
    /// Gets or initializes the service ID for targeting specific services within the HuggingFace provider.
    /// </summary>
    /// <remarks>
    /// This property allows for specifying a particular service or endpoint within the HuggingFace infrastructure.
    /// </remarks>
    public required string ServiceId { get; init; }
}
