using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AITutorSystem.AI.Interfaces;
using AITutorSystem.AI.Providers;

namespace AITutorSystem.AI;

/// <summary>
/// Factory for creating AI provider instances based on configuration.
/// Supports OpenRouter and Claude providers with configuration-based selection.
/// </summary>
public static class AIFactory
{
    /// <summary>
    /// Creates an AI provider instance based on configuration settings.
    /// </summary>
    /// <param name="configuration">Configuration containing AI provider settings.</param>
    /// <param name="httpClient">HTTP client for API requests.</param>
    /// <param name="loggerFactory">Logger factory for creating provider-specific loggers (optional).</param>
    /// <returns>An IAIProvider instance configured for the specified provider.</returns>
    /// <exception cref="ArgumentNullException">Thrown when configuration or httpClient is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when provider configuration is invalid or missing.</exception>
    /// <remarks>
    /// Expected configuration structure:
    /// - AIProvider: "OpenRouter" or "Claude"
    /// - OpenRouter:ApiKey: API key for OpenRouter
    /// - OpenRouter:Model: Model identifier for OpenRouter
    /// - OpenRouter:EmbeddingModel: (Optional) Embedding model for OpenRouter
    /// - Claude:ApiKey: API key for Claude
    /// - Claude:Model: Model identifier for Claude
    /// </remarks>
    public static IAIProvider Create(
        IConfiguration configuration, 
        HttpClient httpClient,
        ILoggerFactory? loggerFactory = null)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));

        var provider = configuration["AIProvider"] 
            ?? throw new InvalidOperationException("AIProvider configuration is missing. Please specify 'OpenRouter' or 'Claude'.");

        return provider.ToLowerInvariant() switch
        {
            "openrouter" => CreateOpenRouterProvider(configuration, httpClient, loggerFactory),
            "claude" => CreateClaudeProvider(configuration, httpClient, loggerFactory),
            _ => throw new InvalidOperationException($"Unknown AI provider: {provider}. Supported providers are 'OpenRouter' and 'Claude'.")
        };
    }

    private static IAIProvider CreateOpenRouterProvider(
        IConfiguration configuration, 
        HttpClient httpClient,
        ILoggerFactory? loggerFactory)
    {
        var apiKey = configuration["OpenRouter:ApiKey"]
            ?? throw new InvalidOperationException("OpenRouter:ApiKey configuration is missing.");

        var model = configuration["OpenRouter:Model"]
            ?? throw new InvalidOperationException("OpenRouter:Model configuration is missing.");

        var embeddingModel = configuration["OpenRouter:EmbeddingModel"];
        var logger = loggerFactory?.CreateLogger<OpenRouterProvider>();

        return new OpenRouterProvider(httpClient, apiKey, model, embeddingModel, logger);
    }

    private static IAIProvider CreateClaudeProvider(
        IConfiguration configuration, 
        HttpClient httpClient,
        ILoggerFactory? loggerFactory)
    {
        var apiKey = configuration["Claude:ApiKey"]
            ?? throw new InvalidOperationException("Claude:ApiKey configuration is missing.");

        var model = configuration["Claude:Model"]
            ?? throw new InvalidOperationException("Claude:Model configuration is missing.");

        var logger = loggerFactory?.CreateLogger<ClaudeProvider>();

        return new ClaudeProvider(httpClient, apiKey, model, logger);
    }
}
