using AITutorSystem.AI.Models;
using AITutorSystem.AI.Providers;
using Xunit;

namespace AITutorSystem.AI.Tests;

/// <summary>
/// Integration tests for OpenRouterProvider with real API calls.
/// These tests are skipped by default. Remove the Skip attribute and add your API key to run them.
/// </summary>
public class OpenRouterProviderIntegrationTests
{
    // TODO: Add your OpenRouter API key here for local testing
    private const string ApiKey = "YOUR_API_KEY_HERE";
    private const string Model = "openai/gpt-3.5-turbo";
    private const string EmbeddingModel = "openai/text-embedding-3-small";

    [Fact(Skip = "Integration test - requires valid API key")]
    public async Task GenerateAsync_WithRealApi_ReturnsValidResponse()
    {
        // Arrange
        var httpClient = new HttpClient();
        var provider = new OpenRouterProvider(httpClient, ApiKey, Model);

        var request = new AIRequest
        {
            SystemPrompt = "You are a helpful assistant.",
            UserPrompt = "Say 'Hello, World!' and nothing else.",
            Temperature = 0.3,
            MaxTokens = 50
        };

        // Act
        var response = await provider.GenerateAsync(request);

        // Assert
        Assert.True(response.Success, $"API call failed: {response.ErrorMessage}");
        Assert.NotEmpty(response.Content);
        Assert.True(response.TokensUsed > 0);
        Assert.NotEmpty(response.Model);
    }

    [Fact(Skip = "Integration test - requires valid API key")]
    public async Task StreamAsync_WithRealApi_YieldsContentChunks()
    {
        // Arrange
        var httpClient = new HttpClient();
        var provider = new OpenRouterProvider(httpClient, ApiKey, Model);

        var request = new AIRequest
        {
            SystemPrompt = "You are a helpful assistant.",
            UserPrompt = "Count from 1 to 5.",
            Temperature = 0.3,
            MaxTokens = 100
        };

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in provider.StreamAsync(request))
        {
            chunks.Add(chunk);
        }

        // Assert
        Assert.NotEmpty(chunks);
        var fullResponse = string.Join("", chunks);
        Assert.NotEmpty(fullResponse);
    }

    [Fact(Skip = "Integration test - requires valid API key")]
    public async Task GenerateEmbeddingAsync_WithRealApi_ReturnsValidEmbedding()
    {
        // Arrange
        var httpClient = new HttpClient();
        var provider = new OpenRouterProvider(httpClient, ApiKey, Model, EmbeddingModel);

        var request = new EmbeddingRequest
        {
            Text = "This is a test sentence for embedding generation."
        };

        // Act
        var response = await provider.GenerateEmbeddingAsync(request);

        // Assert
        Assert.True(response.Success, $"API call failed: {response.ErrorMessage}");
        Assert.NotEmpty(response.Embedding);
        Assert.True(response.Embedding.Length > 0);
        Assert.True(response.TokensUsed > 0);
        Assert.NotEmpty(response.Model);
    }

    [Fact(Skip = "Integration test - requires valid API key")]
    public async Task GenerateAsync_WithInvalidApiKey_ReturnsFailedResponse()
    {
        // Arrange
        var httpClient = new HttpClient();
        var provider = new OpenRouterProvider(httpClient, "invalid-key", Model);

        var request = new AIRequest
        {
            SystemPrompt = "You are a helpful assistant.",
            UserPrompt = "Hello"
        };

        // Act
        var response = await provider.GenerateAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
    }
}
