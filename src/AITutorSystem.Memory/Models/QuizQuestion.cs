namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents a single question in a quiz.
/// </summary>
public class QuizQuestion
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is not QuizQuestion other) return false;
        
        return Id == other.Id &&
               Text == other.Text &&
               Options.SequenceEqual(other.Options) &&
               CorrectAnswer == other.CorrectAnswer &&
               Topic == other.Topic &&
               DifficultyLevel == other.DifficultyLevel;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Text, CorrectAnswer, Topic, DifficultyLevel);
    }
}
