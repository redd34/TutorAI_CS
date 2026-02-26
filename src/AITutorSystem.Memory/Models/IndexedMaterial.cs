namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents educational content that has been processed and catalogued.
/// </summary>
public class IndexedMaterial
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public List<string> Topics { get; set; } = new();
    public string TeachingStyle { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime IndexedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not IndexedMaterial other) return false;
        
        return Id == other.Id &&
               Title == other.Title &&
               Url == other.Url &&
               Channel == other.Channel &&
               Topics.SequenceEqual(other.Topics) &&
               TeachingStyle == other.TeachingStyle &&
               DifficultyLevel == other.DifficultyLevel &&
               Duration == other.Duration &&
               IndexedAt == other.IndexedAt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Title, Url, Channel, TeachingStyle, DifficultyLevel, Duration, IndexedAt);
    }
}
