namespace SemanticKernelPooling;

public record AzureOpenAIConfiguration
{
    public required string UniqueName { get; init; }
    // ReSharper disable UnusedMember.Global
    public required string DeploymentName { get; init; }
    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
    public required string DeploymentTextEmbedding { get; init; }
}