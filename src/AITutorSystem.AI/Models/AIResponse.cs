namespace AITutorSystem.AI.Models;

/// <summary>
/// Represents a response from an AI provider.
/// </summary>
public class AIResponse
{
    /// <summary>
    /// Gets or sets the generated text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of tokens used in the request and response.
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// Gets or sets the model identifier used for generation.
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
}
