namespace AITutorSystem.AI.Models;

/// <summary>
/// Represents a request to an AI provider for text generation.
/// </summary>
public class AIRequest
{
    /// <summary>
    /// Gets or sets the system prompt that defines the AI's role and behavior.
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user prompt containing the actual query or instruction.
    /// </summary>
    public string UserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temperature for response randomness (0.0 = deterministic, 1.0 = creative).
    /// Default is 0.7.
    /// </summary>
    public double Temperature { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// Default is 1000.
    /// </summary>
    public int MaxTokens { get; set; } = 10000;

    /// <summary>
    /// Gets or sets additional provider-specific parameters.
    /// </summary>
    public Dictionary<string, object> AdditionalParameters { get; set; } = new();
}
