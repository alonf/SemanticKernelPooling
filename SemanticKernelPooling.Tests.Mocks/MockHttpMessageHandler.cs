using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SemanticKernelPooling.Tests.Mocks;

/// <summary>
/// Provides a mock HTTP message handler that returns predefined responses for testing purposes.
/// This handler enables testing of HTTP-dependent code without making actual network requests.
/// </summary>
/// <remarks>
/// The handler loads mock responses from JSON files in a specified directory.
/// Each response file should contain a request key, status code, content, and optional headers.
/// </remarks>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, MockResponse> _responses;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="responsesPath">The directory path containing mock response JSON files.</param>
    /// <param name="logger">The logger for recording handler activities.</param>
    public MockHttpMessageHandler(string responsesPath, ILogger logger)
    {
        _logger = logger;
        _responses = LoadMockResponses(responsesPath);
    }

    /// <summary>
    /// Processes HTTP requests and returns mock responses based on the request key.
    /// </summary>
    /// <param name="request">The HTTP request message to process.</param>
    /// <param name="cancellationToken">A token for canceling the request.</param>
    /// <returns>A task representing the HTTP response message.</returns>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestKey = CreateRequestKey(request);
        _logger.LogInformation("Mock HTTP handler received request:");
        _logger.LogInformation("  Method: {Method}", request.Method);
        _logger.LogInformation("  URI: {URI}", request.RequestUri);
        _logger.LogInformation("  Generated Key: {RequestKey}", requestKey);
        _logger.LogInformation("Available mock response keys:");
        foreach (var key in _responses.Keys)
        {
            _logger.LogInformation("  {Key}", key);
        }

        if (_responses.TryGetValue(requestKey, out var mockResponse))
        {
            _logger.LogInformation("Found matching mock response");
            var response = new HttpResponseMessage
            {
                StatusCode = mockResponse.StatusCode,
                Content = new StringContent(mockResponse.Content, System.Text.Encoding.UTF8, "application/json")
            };

            foreach (var header in mockResponse.Headers)
            {
                response.Headers.Add(header.Key, header.Value);
            }

            return Task.FromResult(response);
        }

        _logger.LogWarning("No mock response found for request");
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    private string CreateRequestKey(HttpRequestMessage request)
    {
        return $"{request.Method}:{request.RequestUri?.PathAndQuery}";
    }

    private Dictionary<string, MockResponse> LoadMockResponses(string path)
    {
        var responses = new Dictionary<string, MockResponse>();

        if (Directory.Exists(path))
        {
            foreach (var file in Directory.GetFiles(path, "*.json"))
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var mockResponse = JsonSerializer.Deserialize<MockResponse>(content);
                    if (mockResponse?.RequestKey != null)
                    {
                        responses[mockResponse.RequestKey] = mockResponse;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading mock response from file: {FilePath}", file);
                }
            }
        }

        return responses;
    }
}