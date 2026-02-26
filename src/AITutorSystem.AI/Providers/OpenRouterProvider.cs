using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using AITutorSystem.AI.Interfaces;
using AITutorSystem.AI.Models;

namespace AITutorSystem.AI.Providers;

/// <summary>
/// OpenRouter AI provider implementation.
/// Supports text generation, streaming, and embedding generation via OpenRouter API.
/// </summary>
public class OpenRouterProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _embeddingModel;
    private readonly ILogger<OpenRouterProvider>? _logger;
    private const string BaseUrl = "https://openrouter.ai/api/v1";

    /// <summary>
    /// Initializes a new instance of the OpenRouterProvider.
    /// </summary>
    /// <param name="httpClient">HTTP client for API requests.</param>
    /// <param name="apiKey">OpenRouter API key.</param>
    /// <param name="model">Model identifier for text generation.</param>
    /// <param name="embeddingModel">Model identifier for embeddings (optional).</param>
    /// <param name="logger">Logger for tracking operations (optional).</param>
    public OpenRouterProvider(
        HttpClient httpClient, 
        string apiKey, 
        string model, 
        string? embeddingModel = null,
        ILogger<OpenRouterProvider>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _embeddingModel = embeddingModel ?? "openai/text-embedding-3-small";
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AIResponse> GenerateAsync(AIRequest request)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;

        _logger?.LogInformation(
            "OpenRouter GenerateAsync started. OperationId: {OperationId}, Model: {Model}, Temperature: {Temperature}, MaxTokens: {MaxTokens}",
            operationId, _model, request.Temperature, request.MaxTokens);

        try
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = request.SystemPrompt },
                    new { role = "user", content = request.UserPrompt }
                },
                temperature = request.Temperature,
                max_tokens = request.MaxTokens
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/chat/completions");
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
            httpRequest.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError(
                    "OpenRouter API error. OperationId: {OperationId}, StatusCode: {StatusCode}, Response: {Response}",
                    operationId, response.StatusCode, responseContent);

                return new AIResponse
                {
                    Success = false,
                    ErrorMessage = $"OpenRouter API error: {response.StatusCode} - {responseContent}"
                };
            }

            var jsonResponse = JsonSerializer.Deserialize<OpenRouterResponse>(responseContent);
            
            if (jsonResponse?.Choices == null || jsonResponse.Choices.Length == 0)
            {
                _logger?.LogWarning(
                    "No response content from OpenRouter. OperationId: {OperationId}",
                    operationId);

                return new AIResponse
                {
                    Success = false,
                    ErrorMessage = "No response content from OpenRouter"
                };
            }

            var duration = DateTime.UtcNow - startTime;
            var tokensUsed = jsonResponse.Usage?.TotalTokens ?? 0;

            _logger?.LogInformation(
                "OpenRouter GenerateAsync completed. OperationId: {OperationId}, Model: {Model}, TokensUsed: {TokensUsed}, " +
                "PromptTokens: {PromptTokens}, CompletionTokens: {CompletionTokens}, Duration: {Duration}ms, ResponseLength: {ResponseLength}",
                operationId, jsonResponse.Model ?? _model, tokensUsed,
                jsonResponse.Usage?.PromptTokens ?? 0, jsonResponse.Usage?.CompletionTokens ?? 0,
                duration.TotalMilliseconds, jsonResponse.Choices[0].Message?.Content?.Length ?? 0);

            return new AIResponse
            {
                Content = jsonResponse.Choices[0].Message?.Content ?? string.Empty,
                TokensUsed = tokensUsed,
                Model = jsonResponse.Model ?? _model,
                Success = true
            };
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            
            _logger?.LogError(ex,
                "Error calling OpenRouter API. OperationId: {OperationId}, Duration: {Duration}ms, Error: {Error}",
                operationId, duration.TotalMilliseconds, ex.Message);

            return new AIResponse
            {
                Success = false,
                ErrorMessage = $"Error calling OpenRouter API: {ex.Message}"
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
            "OpenRouter StreamAsync started. OperationId: {OperationId}, Model: {Model}, Temperature: {Temperature}, MaxTokens: {MaxTokens}",
            operationId, _model, request.Temperature, request.MaxTokens);

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            },
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            stream = true
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/chat/completions");
        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
        httpRequest.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogError(
                "OpenRouter streaming error. OperationId: {OperationId}, StatusCode: {StatusCode}",
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
            
            if (data == "[DONE]")
                break;

            var chunk = TryDeserializeStreamChunk(data);
            var content = chunk?.Choices?[0]?.Delta?.Content;
            
            if (!string.IsNullOrEmpty(content))
            {
                totalChunks++;
                yield return content;
            }
        }

        var duration = DateTime.UtcNow - startTime;
        
        _logger?.LogInformation(
            "OpenRouter StreamAsync completed. OperationId: {OperationId}, Model: {Model}, TotalChunks: {TotalChunks}, Duration: {Duration}ms",
            operationId, _model, totalChunks, duration.TotalMilliseconds);
    }

    private static OpenRouterStreamChunk? TryDeserializeStreamChunk(string data)
    {
        try
        {
            return JsonSerializer.Deserialize<OpenRouterStreamChunk>(data);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<EmbeddingResponse> GenerateEmbeddingAsync(EmbeddingRequest request)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;
        var embeddingModel = request.Model ?? _embeddingModel;

        _logger?.LogInformation(
            "OpenRouter GenerateEmbeddingAsync started. OperationId: {OperationId}, Model: {Model}, TextLength: {TextLength}",
            operationId, embeddingModel, request.Text.Length);

        try
        {
            var requestBody = new
            {
                model = embeddingModel,
                input = request.Text
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/embeddings");
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
            httpRequest.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError(
                    "OpenRouter embedding API error. OperationId: {OperationId}, StatusCode: {StatusCode}, Response: {Response}",
                    operationId, response.StatusCode, responseContent);

                return new EmbeddingResponse
                {
                    Success = false,
                    ErrorMessage = $"OpenRouter API error: {response.StatusCode} - {responseContent}"
                };
            }

            var jsonResponse = JsonSerializer.Deserialize<OpenRouterEmbeddingResponse>(responseContent);
            
            if (jsonResponse?.Data == null || jsonResponse.Data.Length == 0)
            {
                _logger?.LogWarning(
                    "No embedding data from OpenRouter. OperationId: {OperationId}",
                    operationId);

                return new EmbeddingResponse
                {
                    Success = false,
                    ErrorMessage = "No embedding data from OpenRouter"
                };
            }

            var duration = DateTime.UtcNow - startTime;
            var tokensUsed = jsonResponse.Usage?.TotalTokens ?? 0;
            var embeddingDimensions = jsonResponse.Data[0].Embedding?.Length ?? 0;

            _logger?.LogInformation(
                "OpenRouter GenerateEmbeddingAsync completed. OperationId: {OperationId}, Model: {Model}, TokensUsed: {TokensUsed}, " +
                "EmbeddingDimensions: {EmbeddingDimensions}, Duration: {Duration}ms",
                operationId, jsonResponse.Model ?? embeddingModel, tokensUsed, embeddingDimensions, duration.TotalMilliseconds);

            return new EmbeddingResponse
            {
                Embedding = jsonResponse.Data[0].Embedding ?? Array.Empty<float>(),
                Model = jsonResponse.Model ?? embeddingModel,
                TokensUsed = tokensUsed,
                Success = true
            };
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            
            _logger?.LogError(ex,
                "Error calling OpenRouter embedding API. OperationId: {OperationId}, Duration: {Duration}ms, Error: {Error}",
                operationId, duration.TotalMilliseconds, ex.Message);

            return new EmbeddingResponse
            {
                Success = false,
                ErrorMessage = $"Error calling OpenRouter API: {ex.Message}"
            };
        }
    }

    #region Response Models

    private class OpenRouterResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public Choice[]? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    private class OpenRouterStreamChunk
    {
        [JsonPropertyName("choices")]
        public StreamChoice[]? Choices { get; set; }
    }

    private class StreamChoice
    {
        [JsonPropertyName("delta")]
        public Delta? Delta { get; set; }
    }

    private class Delta
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class OpenRouterEmbeddingResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("data")]
        public EmbeddingData[]? Data { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    #endregion
}
