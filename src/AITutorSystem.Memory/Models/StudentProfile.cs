namespace AITutorSystem.Memory.Models;

/// <summary>
/// Comprehensive record of a student's knowledge level and learning progress.
/// References to completed tasks, quiz history, and learning plans are stored separately.
/// </summary>
public class StudentProfile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// List of learning plan IDs associated with this student.
    /// Student can have multiple learning plans and work on any of them.
    /// </summary>
    public List<string> LearningPlanIds { get; set; } = new();
    
    /// <summary>
    /// The currently active learning plan ID that the student is working on.
    /// </summary>
    public string? ActiveLearningPlanId { get; set; }
    
    /// <summary>
    /// References to completed task IDs. Actual CompletedTask objects stored separately.
    /// </summary>
    public List<string> CompletedTaskIds { get; set; } = new();
    
    /// <summary>
    /// References to quiz result IDs. Actual QuizResult objects stored separately.
    /// </summary>
    public List<string> QuizResultIds { get; set; } = new();
    
    /// <summary>
    /// Topic-based knowledge level tracking (Topic -> Level).
    /// </summary>
    public Dictionary<string, int> KnowledgeLevel { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not StudentProfile other) return false;
        
        return Id == other.Id &&
               Name == other.Name &&
               LearningPlanIds.SequenceEqual(other.LearningPlanIds) &&
               ActiveLearningPlanId == other.ActiveLearningPlanId &&
               CompletedTaskIds.SequenceEqual(other.CompletedTaskIds) &&
               QuizResultIds.SequenceEqual(other.QuizResultIds) &&
               KnowledgeLevel.Count == other.KnowledgeLevel.Count &&
               KnowledgeLevel.All(kvp => other.KnowledgeLevel.TryGetValue(kvp.Key, out var value) && value == kvp.Value) &&
               CreatedAt == other.CreatedAt &&
               UpdatedAt == other.UpdatedAt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, ActiveLearningPlanId, CreatedAt, UpdatedAt);
    }
}
