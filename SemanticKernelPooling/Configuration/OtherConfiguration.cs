namespace SemanticKernelPooling.Configuration;

public record OtherConfiguration : AIServiceProviderConfiguration
{
    public required string ModelId { get; init; }
    public required string ApiKey { get; init; }
    public required string Endpoint { get; init; }
    public required string ServiceId { get; init; }
}