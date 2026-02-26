namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents a task that has been completed by a student.
/// Stored separately and referenced by StudentProfile.
/// </summary>
public class CompletedTask
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string LearningPlanId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public int AttemptsCount { get; set; }
    
    /// <summary>
    /// Final quiz result ID that marked this task as complete.
    /// </summary>
    public string? FinalQuizResultId { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not CompletedTask other) return false;
        
        return Id == other.Id &&
               StudentId == other.StudentId &&
               TaskId == other.TaskId &&
               LearningPlanId == other.LearningPlanId &&
               CompletedAt == other.CompletedAt &&
               AttemptsCount == other.AttemptsCount &&
               FinalQuizResultId == other.FinalQuizResultId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, StudentId, TaskId, LearningPlanId, CompletedAt, AttemptsCount);
    }
}
