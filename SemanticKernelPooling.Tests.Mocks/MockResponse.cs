using System.Net;

namespace SemanticKernelPooling.Tests.Mocks;

/// <summary>
/// Represents a mock HTTP response configuration that can be stored in a JSON file.
/// </summary>
public class MockResponse
{
    /// <summary>
    /// Gets or sets the key used to match incoming requests to this response.
    /// </summary>
    /// <remarks>
    /// The key format is typically "{HTTP_METHOD}:{PATH}". For example, "POST:/v1/chat/completions".
    /// </remarks>
    public string RequestKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP status code to return in the mock response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the content body of the mock response.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP headers to include in the mock response.
    /// </summary>
    public Dictionary<string, string[]> Headers { get; set; } = new();
}