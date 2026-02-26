namespace AITutorSystem.AI.Models;

/// <summary>
/// Represents a response containing vector embeddings.
/// </summary>
public class EmbeddingResponse
{
    /// <summary>
    /// Gets or sets the vector embedding as an array of floats.
    /// </summary>
    public float[] Embedding { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Gets or sets the model used for embedding generation.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if the request failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens used in the embedding request.
    /// </summary>
    public int TokensUsed { get; set; }
}
