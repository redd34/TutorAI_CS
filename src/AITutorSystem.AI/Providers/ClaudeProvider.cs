using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using AITutorSystem.AI.Interfaces;
using AITutorSystem.AI.Models;

namespace AITutorSystem.AI.Providers;

/// <summary>
/// Claude (Anthropic) AI provider implementation.
/// Supports text generation and streaming via Claude API.
/// Note: Claude does not natively support embeddings; use OpenRouter or another provider for embeddings.
/// </summary>
public class ClaudeProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<ClaudeProvider>? _logger;
    private const string BaseUrl = "https://api.anthropic.com/v1";
    private const string ApiVersion = "2023-06-01";

    /// <summary>
    /// Initializes a new instance of the ClaudeProvider.
    /// </summary>
    /// <param name="httpClient">HTTP client for API requests.</param>
    /// <param name="apiKey">Anthropic API key.</param>
    /// <param name="model">Model identifier (e.g., claude-3-5-sonnet-20241022).</param>
    /// <param name="logger">Logger for tracking operations (optional).</param>
    public ClaudeProvider(
        HttpClient httpClient, 
        string apiKey, 
        string model,
        ILogger<ClaudeProvider>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AIResponse> GenerateAsync(AIRequest request)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;

        _logger?.LogInformation(
            "Claude GenerateAsync started. OperationId: {OperationId}, Model: {Model}, Temperature: {Temperature}, MaxTokens: {MaxTokens}",
            operationId, _model, request.Temperature, request.MaxTokens);

        try
        {
            var requestBody = new
            {
                model = _model,
                max_tokens = request.MaxTokens,
                temperature = request.Temperature,
                system = request.SystemPrompt,
                messages = new[]
                {
                    new { role = "user", content = request.UserPrompt }
                }
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/messages");
            httpRequest.Headers.Add("x-api-key", _apiKey);
            httpRequest.Headers.Add("anthropic-version", ApiVersion);
            httpRequest.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError(
                    "Claude API error. OperationId: {OperationId}, StatusCode: {StatusCode}, Response: {Response}",
                    operationId, response.StatusCode, responseContent);

                return new AIResponse
                {
                    Success = false,
                    ErrorMessage = $"Claude API error: {response.StatusCode} - {responseContent}"
                };
            }

            var jsonResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent);
            
            if (jsonResponse?.Content == null || jsonResponse.Content.Length == 0)
            {
                _logger?.LogWarning(
                    "No response content from Claude. OperationId: {OperationId}",
                    operationId);

                return new AIResponse
                {
                    Success = false,
                    ErrorMessage = "No response content from Claude"
                };
            }

            var textContent = jsonResponse.Content
                .Where(c => c.Type == "text")
                .Select(c => c.Text)
                .FirstOrDefault() ?? string.Empty;

            var duration = DateTime.UtcNow - startTime;
            var inputTokens = jsonResponse.Usage?.InputTokens ?? 0;
            var outputTokens = jsonResponse.Usage?.OutputTokens ?? 0;
            var totalTokens = inputTokens + outputTokens;

            _logger?.LogInformation(
                "Claude GenerateAsync completed. OperationId: {OperationId}, Model: {Model}, RequestId: {RequestId}, " +
                "InputTokens: {InputTokens}, OutputTokens: {OutputTokens}, TotalTokens: {TotalTokens}, " +
                "Duration: {Duration}ms, ResponseLength: {ResponseLength}, StopReason: {StopReason}",
                operationId, jsonResponse.Model ?? _model, jsonResponse.Id,
                inputTokens, outputTokens, totalTokens,
                duration.TotalMilliseconds, textContent.Length, jsonResponse.StopReason);

            return new AIResponse
            {
                Content = textContent,
                TokensUsed = totalTokens,
                Model = jsonResponse.Model ?? _model,
                Success = true
            };
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            
            _logger?.LogError(ex,
                "Error calling Claude API. OperationId: {OperationId}, Duration: {Duration}ms, Error: {Error}",
                operationId, duration.TotalMilliseconds, ex.Message);

            return new AIResponse
            {
                Success = false,
                ErrorMessage = $"Error calling Claude API: {ex.Message}"
            };
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamAsync(AIRequest request)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;
        var totalChunks = 0;

        _logger?.LogInformation(
            "Claude StreamAsync started. OperationId: {OperationId}, Model: {Model}, Temperature: {Temperature}, MaxTokens: {MaxTokens}",
            operationId, _model, request.Temperature, request.MaxTokens);

        var requestBody = new
        {
            model = _model,
            max_tokens = request.MaxTokens,
            temperature = request.Temperature,
            system = request.SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = request.UserPrompt }
            },
            stream = true
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/messages");
        httpRequest.Headers.Add("x-api-key", _apiKey);
        httpRequest.Headers.Add("anthropic-version", ApiVersion);
        httpRequest.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogError(
                "Claude streaming error. OperationId: {OperationId}, StatusCode: {StatusCode}",
                operationId, response.StatusCode);

            yield return $"Error: {response.StatusCode}";
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var data = line.Substring(6);

            var chunk = TryDeserializeStreamEvent(data);
            
            if (chunk?.Type == "content_block_delta" && chunk.Delta?.Type == "text_delta")
            {
                var content = chunk.Delta.Text;
                if (!string.IsNullOrEmpty(content))
                {
                    totalChunks++;
                    yield return content;
                }
            }
        }

        var duration = DateTime.UtcNow - startTime;
        
        _logger?.LogInformation(
            "Claude StreamAsync completed. OperationId: {OperationId}, Model: {Model}, TotalChunks: {TotalChunks}, Duration: {Duration}ms",
            operationId, _model, totalChunks, duration.TotalMilliseconds);
    }

    private static ClaudeStreamEvent? TryDeserializeStreamEvent(string data)
    {
        try
        {
            return JsonSerializer.Deserialize<ClaudeStreamEvent>(data);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public Task<EmbeddingResponse> GenerateEmbeddingAsync(EmbeddingRequest request)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        
        _logger?.LogWarning(
            "Claude embedding request attempted. OperationId: {OperationId}, TextLength: {TextLength}. " +
            "Claude does not support embeddings - use OpenRouter or another provider.",
            operationId, request.Text.Length);

        // Claude does not natively support embeddings
        // Return an error response indicating this limitation
        return Task.FromResult(new EmbeddingResponse
        {
            Success = false,
            ErrorMessage = "Claude does not support embedding generation. Please use OpenRouter or another provider for embeddings."
        });
    }

    #region Response Models

    private class ClaudeResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public ContentBlock[]? Content { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }

        [JsonPropertyName("usage")]
        public ClaudeUsage? Usage { get; set; }
    }

    private class ContentBlock
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class ClaudeUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }

    private class ClaudeStreamEvent
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("index")]
        public int? Index { get; set; }

        [JsonPropertyName("delta")]
        public StreamDelta? Delta { get; set; }
    }

    private class StreamDelta
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    #endregion
}
