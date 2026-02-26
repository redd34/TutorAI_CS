namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents a personalized sequence of tasks based on baseline assessment.
/// </summary>
public class LearningPlan
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public List<LearningTask> Tasks { get; set; } = new();
    public string CurrentTaskId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not LearningPlan other) return false;
        
        return Id == other.Id &&
               StudentId == other.StudentId &&
               Tasks.SequenceEqual(other.Tasks) &&
               CurrentTaskId == other.CurrentTaskId &&
               CreatedAt == other.CreatedAt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, StudentId, CurrentTaskId, CreatedAt);
    }
}
