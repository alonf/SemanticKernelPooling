namespace SemanticKernelPooling.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Global

public abstract record AIServiceProviderConfiguration
{
    public required string UniqueName { get; init; }
    public required string ServiceType { get; init; } // e.g., "AzureOpenAI", "OpenAI", etc.
    public int InstanceCount { get; init; }
    //User can ask for kernel using the usage key, so we can create different kernel providers for the same usage key
    //for example, use OpenAI and AzureOpenAI for the same task
    public string UsageKey { get; set; } = string.Empty;  // Optional key for additional identification
    public required string DeploymentTextEmbedding { get; init; }
    public int MaxWaitForKernelInSeconds { get; set; }
}