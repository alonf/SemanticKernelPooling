using SemanticKernelPooling.Configuration;

namespace SemanticKernelPooling.Connectors.OpenAI;

// ReSharper disable once ClassNeverInstantiated.Global
public record AzureOpenAIConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required string DeploymentName { get; init; }
    public required string ApiKey { get; init; }
    public required string ModelId { get; init; }
    public required string Endpoint { get; init; }
    public required string ServiceId { get; init; }
}