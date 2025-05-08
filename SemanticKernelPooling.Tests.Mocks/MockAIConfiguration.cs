namespace SemanticKernelPooling.Tests.Mocks;

/// <summary>
/// Represents a mock configuration for testing kernel pool behavior.
/// This configuration simulates the settings required for AI service providers without actual credentials.
/// </summary>
/// <remarks>
/// This mock configuration is used in unit tests to verify the kernel pooling behavior
/// without requiring actual AI service provider credentials or connections.
/// </remarks>
public record MockAIConfiguration : AIServiceProviderConfiguration
{
    /// <summary>
    /// Gets or initializes the API key used for testing.
    /// In mock scenarios, this can be any non-null string.
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets or initializes the model identifier used for testing.
    /// In mock scenarios, this can be any non-null string.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets or initializes the service identifier used for testing.
    /// In mock scenarios, this can be any non-null string.
    /// </summary>
    public required string ServiceId { get; init; }

    /// <summary>
    /// Gets or initializes the endpoint URL used for testing.
    /// In mock scenarios, this can be any valid URL string.
    /// </summary>
    public required string Endpoint { get; init; }
}