using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.Connectors.MistralAI;

public record MistralAIConfiguration : AIServiceProviderConfiguration
{
    public required string ModelId { get; init; }
    public string? Endpoint { get; init; }
    public required string ApiKey { get; init; }
    public required string ServiceId { get; init; }
}