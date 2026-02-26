namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents a personalized sequence of tasks based on baseline assessment.
/// Each plan is associated with a specific learning path.
/// </summary>
public class LearningPlan
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference to the learning path this plan is based on.
    /// </summary>
    public string LearningPathId { get; set; } = string.Empty;
    
    /// <summary>
    /// Prioritized list of tasks for this learning plan.
    /// </summary>
    public List<LearningTask> Tasks { get; set; } = new();
    
    /// <summary>
    /// The current task the student is working on.
    /// </summary>
    public string CurrentTaskId { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if this plan is currently active or completed.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not LearningPlan other) return false;
        
        return Id == other.Id &&
               StudentId == other.StudentId &&
               LearningPathId == other.LearningPathId &&
               Tasks.SequenceEqual(other.Tasks) &&
               CurrentTaskId == other.CurrentTaskId &&
               IsActive == other.IsActive &&
               CreatedAt == other.CreatedAt &&
               CompletedAt == other.CompletedAt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, StudentId, LearningPathId, CurrentTaskId, IsActive, CreatedAt);
    }
}
