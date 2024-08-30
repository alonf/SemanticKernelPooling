namespace SemanticKernelPooling.Configuration;

public record GoogleConfiguration : AIServiceProviderConfiguration
{
    public required string ModelId { get; init; }
    public required string ApiKey { get; init; }
    public required string ApiVersion { get; init; }
    public required string ServiceId { get; init; }
}