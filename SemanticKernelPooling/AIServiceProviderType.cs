namespace SemanticKernelPooling;

/// <summary>
/// Specifies the different types of AI service providers supported by the Semantic Kernel Pooling framework.
/// </summary>
public enum AIServiceProviderType
{
    /// <summary>
    /// Represents the Azure OpenAI service provider.
    /// </summary>
    AzureOpenAI,

    /// <summary>
    /// Represents the OpenAI service provider.
    /// </summary>
    OpenAI,

    /// <summary>
    /// Represents the Mistral AI service provider.
    /// </summary>
    MistralAI,

    /// <summary>
    /// Represents the Google AI service provider.
    /// </summary>
    Google,

    /// <summary>
    /// Represents the HuggingFace AI service provider.
    /// </summary>
    HuggingFace,

    /// <summary>
    /// Represents any other AI service provider not specifically listed.
    /// </summary>
    OtherAI
}
