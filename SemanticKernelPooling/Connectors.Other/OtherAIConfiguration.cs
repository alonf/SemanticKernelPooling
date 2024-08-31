namespace SemanticKernelPooling.Connectors.Other;


/// <summary>
/// Represents the configuration settings required for an Other AI service provider.
/// </summary>
/// <remarks>
/// This record inherits from <see cref="AIServiceProviderConfiguration"/> and adds specific settings 
/// required to connect to and use an Other AI service provider, including model ID, API key, endpoint, and service ID.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
public record OtherAIConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    /// <summary>
    /// Gets or initializes the ID of the model to use with the Other AI service.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets or initializes the API key required to authenticate requests to the Other AI service.
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets or initializes the endpoint URL of the Other AI service.
    /// </summary>
    public required string Endpoint { get; init; }

    /// <summary>
    /// Gets or initializes the service ID for targeting specific services within the Other AI provider.
    /// </summary>
    public required string ServiceId { get; init; }
}
