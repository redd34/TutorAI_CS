namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents a predefined curriculum for a subject with goals and ordered tasks.
/// </summary>
public class LearningPath
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Goals { get; set; } = new();
    public List<LearningTask> Tasks { get; set; } = new();
    public string DifficultyLevel { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is not LearningPath other) return false;
        
        return Id == other.Id &&
               Name == other.Name &&
               Description == other.Description &&
               Goals.SequenceEqual(other.Goals) &&
               Tasks.SequenceEqual(other.Tasks) &&
               DifficultyLevel == other.DifficultyLevel;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Description, DifficultyLevel);
    }
}
