using System.Net;
using System.Text;
using System.Text.Json;
using AITutorSystem.AI.Models;
using AITutorSystem.AI.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace AITutorSystem.AI.Tests;

/// <summary>
/// Unit tests for OpenRouterProvider.
/// </summary>
public class OpenRouterProviderTests
{
    private const string TestApiKey = "test-api-key";
    private const string TestModel = "openai/gpt-4";
    private const string TestEmbeddingModel = "openai/text-embedding-3-small";

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new OpenRouterProvider(null!, TestApiKey, TestModel));
    }

    [Fact]
    public void Constructor_WithNullApiKey_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new OpenRouterProvider(httpClient, null!, TestModel));
    }

    [Fact]
    public void Constructor_WithNullModel_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new OpenRouterProvider(httpClient, TestApiKey, null!));
    }

    [Fact]
    public async Task GenerateAsync_WithSuccessfulResponse_ReturnsSuccessfulAIResponse()
    {
        // Arrange
        var mockResponse = new
        {
            id = "test-id",
            model = TestModel,
            choices = new[]
            {
                new
                {
                    message = new { role = "assistant", content = "Test response content" },
                    finish_reason = "stop"
                }
            },
            usage = new
            {
                prompt_tokens = 10,
                completion_tokens = 20,
                total_tokens = 30
            }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = new OpenRouterProvider(httpClient, TestApiKey, TestModel);

        var request = new AIRequest
        {
            SystemPrompt = "You are a helpful assistant.",
            UserPrompt = "Hello, world!",
            Temperature = 0.7,
            MaxTokens = 100
        };

        // Act
        var response = await provider.GenerateAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Test response content", response.Content);
        Assert.Equal(30, response.TokensUsed);
        Assert.Equal(TestModel, response.Model);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task GenerateAsync_WithApiError_ReturnsFailedResponse()
    {
        // Arrange
        var errorResponse = new { error = "Invalid API key" };
        var httpClient = CreateMockHttpClient(HttpStatusCode.Unauthorized, errorResponse);
        var provider = new OpenRouterProvider(httpClient, TestApiKey, TestModel);

        var request = new AIRequest
        {
            SystemPrompt = "You are a helpful assistant.",
            UserPrompt = "Hello, world!"
        };

        // Act
        var response = await provider.GenerateAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("Unauthorized", response.ErrorMessage);
    }

    [Fact]
    public async Task GenerateAsync_WithEmptyChoices_ReturnsFailedResponse()
    {
        // Arrange
        var mockResponse = new
        {
            id = "test-id",
            model = TestModel,
            choices = Array.Empty<object>(),
            usage = new { prompt_tokens = 10, completion_tokens = 0, total_tokens = 10 }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = new OpenRouterProvider(httpClient, TestApiKey, TestModel);

        var request = new AIRequest
        {
            SystemPrompt = "You are a helpful assistant.",
            UserPrompt = "Hello, world!"
        };

        // Act
        var response = await provider.GenerateAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("No response content", response.ErrorMessage);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithSuccessfulResponse_ReturnsSuccessfulEmbeddingResponse()
    {
        // Arrange
        var mockEmbedding = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };
        var mockResponse = new
        {
            model = TestEmbeddingModel,
            data = new[]
            {
                new { embedding = mockEmbedding, index = 0 }
            },
            usage = new { prompt_tokens = 5, total_tokens = 5 }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var provider = new OpenRouterProvider(httpClient, TestApiKey, TestModel, TestEmbeddingModel);

        var request = new EmbeddingRequest
        {
            Text = "Test text for embedding"
        };

        // Act
        var response = await provider.GenerateEmbeddingAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(mockEmbedding, response.Embedding);
        Assert.Equal(5, response.TokensUsed);
        Assert.Equal(TestEmbeddingModel, response.Model);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithApiError_ReturnsFailedResponse()
    {
        // Arrange
        var errorResponse = new { error = "Rate limit exceeded" };
        var httpClient = CreateMockHttpClient(HttpStatusCode.TooManyRequests, errorResponse);
        var provider = new OpenRouterProvider(httpClient, TestApiKey, TestModel, TestEmbeddingModel);

        var request = new EmbeddingRequest
        {
            Text = "Test text for embedding"
        };

        // Act
        var response = await provider.GenerateEmbeddingAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("TooManyRequests", response.ErrorMessage);
    }

    [Fact]
    public async Task StreamAsync_WithSuccessfulResponse_YieldsContentChunks()
    {
        // Arrange
        var streamData = new[]
        {
            "data: {\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}\n",
            "data: {\"choices\":[{\"delta\":{\"content\":\" world\"}}]}\n",
            "data: {\"choices\":[{\"delta\":{\"content\":\"!\"}}]}\n",
            "data: [DONE]\n"
        };

        var httpClient = CreateMockStreamingHttpClient(streamData);
        var provider = new OpenRouterProvider(httpClient, TestApiKey, TestModel);

        var request = new AIRequest
        {
            SystemPrompt = "You are a helpful assistant.",
            UserPrompt = "Say hello"
        };

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in provider.StreamAsync(request))
        {
            chunks.Add(chunk);
        }

        // Assert
        Assert.Equal(3, chunks.Count);
        Assert.Equal("Hello", chunks[0]);
        Assert.Equal(" world", chunks[1]);
        Assert.Equal("!", chunks[2]);
    }

    [Fact(Skip = "Real API integration test - Add your API key and remove Skip attribute to run")]
    public async Task GenerateAsync_WithRealHttpClient_ReturnsValidResponse()
    {
        // TODO: Replace with your actual OpenRouter API key
        const string apiKey = "YOUR_API_KEY_HERE";
        
        // Uncomment the following line and comment out the Skip attribute above to run this test
        // Skip.If(apiKey == "YOUR_API_KEY_HERE", "API key not configured");

        // Arrange - Using a REAL HttpClient, not a mock
        var httpClient = new HttpClient();
        var provider = new OpenRouterProvider(httpClient, apiKey, "openai/gpt-3.5-turbo");

        var request = new AIRequest
        {
            SystemPrompt = "You are a helpful assistant that responds concisely.",
            UserPrompt = "Say 'Hello, World!' and nothing else.",
            Temperature = 0.3,
            MaxTokens = 50
        };

        // Act - This makes a REAL API call to OpenRouter
        var response = await provider.GenerateAsync(request);

        // Assert
        Assert.True(response.Success, $"API call failed: {response.ErrorMessage}");
        Assert.NotEmpty(response.Content);
        Assert.Contains("Hello", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.True(response.TokensUsed > 0);
        Assert.NotEmpty(response.Model);
        
        // Cleanup
        httpClient.Dispose();
    }

    /// <summary>
    /// Helper method to create a mock HttpClient with predefined response.
    /// </summary>
    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object responseContent)
    {
        var json = JsonSerializer.Serialize(responseContent);
        var mockHandler = new Mock<HttpMessageHandler>();
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        return new HttpClient(mockHandler.Object);
    }

    /// <summary>
    /// Helper method to create a mock HttpClient for streaming responses.
    /// </summary>
    private static HttpClient CreateMockStreamingHttpClient(string[] streamLines)
    {
        var streamContent = string.Join("", streamLines);
        var mockHandler = new Mock<HttpMessageHandler>();
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(streamContent, Encoding.UTF8, "text/event-stream")
            });

        return new HttpClient(mockHandler.Object);
    }
}
