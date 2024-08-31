namespace SemanticKernelPooling.Connectors.OpenAI;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Represents the configuration settings required for the OpenAI service provider.
/// </summary>
/// <remarks>
/// This record inherits from <see cref="AIServiceProviderConfiguration"/> and includes specific settings 
/// required to connect to and use OpenAI services, such as the API key, model ID, optional organization ID, and service ID.
/// </remarks>
public record OpenAIConfiguration : AIServiceProviderConfiguration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    /// <summary>
    /// Gets or initializes the API key required to authenticate requests to the OpenAI service.
    /// </summary>
    /// <remarks>
    /// This property is required to authenticate with the OpenAI service and authorize API requests.
    /// </remarks>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets or initializes the model ID for the OpenAI service.
    /// </summary>
    /// <remarks>
    /// This property specifies the unique identifier for the model that will be used in requests to the OpenAI service.
    /// </remarks>
    public required string ModelId { get; init; }

    /// <summary>
    /// Gets or sets the organization ID for the OpenAI service.
    /// </summary>
    /// <remarks>
    /// This property is optional and can be used to specify the organization ID associated with the OpenAI account. 
    /// It is useful when accessing resources that belong to a specific organization within OpenAI.
    /// </remarks>
    public string? OrgId { get; set; }

    /// <summary>
    /// Gets or sets the service ID for targeting specific services within the OpenAI provider.
    /// </summary>
    /// <remarks>
    /// This property is optional and allows for specifying a particular service or endpoint within the OpenAI infrastructure.
    /// </remarks>
    public string? ServiceId { get; set; }
}