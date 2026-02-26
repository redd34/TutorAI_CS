namespace AITutorSystem.AI.Models;

/// <summary>
/// Represents a request to generate vector embeddings for text.
/// </summary>
public class EmbeddingRequest
{
    /// <summary>
    /// Gets or sets the text to generate embeddings for.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model to use for embedding generation.
    /// If not specified, the provider's default embedding model will be used.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets additional provider-specific parameters.
    /// </summary>
    public Dictionary<string, object> AdditionalParameters { get; set; } = new();
}
