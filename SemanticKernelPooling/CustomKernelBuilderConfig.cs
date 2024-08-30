namespace SemanticKernelPooling;

// ReSharper disable once ClassNeverInstantiated.Global
public class CustomKernelBuilderConfig
{
    public HttpClient? HttpClient { get; set; } = null;
    public bool ShouldAutoAddChatCompletionService { get; set; } = true;

}