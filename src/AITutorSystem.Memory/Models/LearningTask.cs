namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents a single learning task within a learning path.
/// </summary>
public class LearningTask
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Topics { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public int Order { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not LearningTask other) return false;
        
        return Id == other.Id &&
               Title == other.Title &&
               Description == other.Description &&
               Topics.SequenceEqual(other.Topics) &&
               Prerequisites.SequenceEqual(other.Prerequisites) &&
               Order == other.Order;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Title, Description, Order);
    }
}
