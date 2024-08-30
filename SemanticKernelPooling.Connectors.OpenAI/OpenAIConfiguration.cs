using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.Connectors.OpenAI;

// ReSharper disable once ClassNeverInstantiated.Global
public record OpenAIConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required string ApiKey { get; init; }
    public required string ModelId { get; init; }
    public string? OrgId { get; set; }
    public string? ServiceId { get; set; }
}