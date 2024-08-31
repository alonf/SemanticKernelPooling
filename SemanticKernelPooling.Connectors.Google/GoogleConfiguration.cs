namespace SemanticKernelPooling.Connectors.Google;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Represents the configuration settings required for the Google AI service provider.
/// </summary>
/// <remarks>
/// This record inherits from <see cref="AIServiceProviderConfiguration"/> and adds specific settings 
/// required to connect to and use Google AI services, including model ID, API key, API version, and service ID.
/// </remarks>
public record GoogleConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    /// <summary>
    /// Gets or initializes the ID of the model to use with the Google AI service.
    /// </summary>
    /// <remarks>
    /// This property specifies the unique identifier for the model that will be used in requests to the Google AI service.
    /// </remarks>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets or initializes the API key required to authenticate requests to the Google AI service.
    /// </summary>
    /// <remarks>
    /// This property is required to authenticate with the Google AI service and authorize API requests.
    /// </remarks>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets or initializes the version of the API to use with the Google AI service.
    /// </summary>
    /// <remarks>
    /// This property specifies the version of the Google AI API to target, such as "v1" or "v2beta".
    /// </remarks>
    public required string ApiVersion { get; init; }

    /// <summary>
    /// Gets or initializes the service ID for targeting specific services within the Google AI provider.
    /// </summary>
    /// <remarks>
    /// This property allows for specifying a particular service or endpoint within the Google AI infrastructure.
    /// </remarks>
    public required string ServiceId { get; init; }
}