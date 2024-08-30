namespace SemanticKernelPooling.Configuration;

public record HuggingFaceConfiguration : AIServiceProviderConfiguration
{
    public required string Model { get; init; }
    public required string ApiKey { get; init; }
    public required string Endpoint { get; init; }
    public required string ServiceId { get; init; }
}