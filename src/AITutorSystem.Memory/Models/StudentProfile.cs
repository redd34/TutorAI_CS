namespace AITutorSystem.Memory.Models;

/// <summary>
/// Comprehensive record of a student's knowledge level, completed tasks, and skill assessments.
/// </summary>
public class StudentProfile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CurrentLearningPathId { get; set; } = string.Empty;
    public LearningPath? LearningPath { get; set; }
    public LearningPlan? CurrentPlan { get; set; }
    public List<CompletedTask> CompletedTasks { get; set; } = new();
    public List<QuizResult> QuizHistory { get; set; } = new();
    public Dictionary<string, int> KnowledgeLevel { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not StudentProfile other) return false;
        
        return Id == other.Id &&
               Name == other.Name &&
               CurrentLearningPathId == other.CurrentLearningPathId &&
               Equals(LearningPath, other.LearningPath) &&
               Equals(CurrentPlan, other.CurrentPlan) &&
               CompletedTasks.SequenceEqual(other.CompletedTasks) &&
               QuizHistory.SequenceEqual(other.QuizHistory) &&
               KnowledgeLevel.Count == other.KnowledgeLevel.Count &&
               KnowledgeLevel.All(kvp => other.KnowledgeLevel.TryGetValue(kvp.Key, out var value) && value == kvp.Value) &&
               CreatedAt == other.CreatedAt &&
               UpdatedAt == other.UpdatedAt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, CurrentLearningPathId, CreatedAt, UpdatedAt);
    }
}
