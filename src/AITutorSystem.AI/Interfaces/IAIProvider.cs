using AITutorSystem.AI.Models;

namespace AITutorSystem.AI.Interfaces;

/// <summary>
/// Defines the contract for AI provider implementations.
/// Supports text generation, streaming responses, and embedding generation.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Generates a text response from the AI provider.
    /// </summary>
    /// <param name="request">The AI request containing prompts and parameters.</param>
    /// <returns>The AI response containing generated content and metadata.</returns>
    Task<AIResponse> GenerateAsync(AIRequest request);

    /// <summary>
    /// Streams a text response from the AI provider in real-time chunks.
    /// </summary>
    /// <param name="request">The AI request containing prompts and parameters.</param>
    /// <returns>An async enumerable of text chunks as they arrive.</returns>
    IAsyncEnumerable<string> StreamAsync(AIRequest request);

    /// <summary>
    /// Generates vector embeddings for the given text.
    /// </summary>
    /// <param name="request">The embedding request containing text to embed.</param>
    /// <returns>The embedding response containing the vector representation.</returns>
    Task<EmbeddingResponse> GenerateEmbeddingAsync(EmbeddingRequest request);
}
