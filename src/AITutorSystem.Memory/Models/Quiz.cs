namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents a quiz (baseline or progress assessment).
/// </summary>
public class Quiz
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string? TaskId { get; set; }
    public List<QuizQuestion> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Quiz other) return false;
        
        return Id == other.Id &&
               Type == other.Type &&
               StudentId == other.StudentId &&
               TaskId == other.TaskId &&
               Questions.SequenceEqual(other.Questions) &&
               CreatedAt == other.CreatedAt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Type, StudentId, TaskId, CreatedAt);
    }
}
