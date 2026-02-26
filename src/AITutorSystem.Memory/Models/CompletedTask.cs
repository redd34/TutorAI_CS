namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents a task that has been completed by a student.
/// </summary>
public class CompletedTask
{
    public string TaskId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public int AttemptsCount { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not CompletedTask other) return false;
        
        return TaskId == other.TaskId &&
               CompletedAt == other.CompletedAt &&
               AttemptsCount == other.AttemptsCount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TaskId, CompletedAt, AttemptsCount);
    }
}
