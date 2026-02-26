namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents an identified gap in student knowledge.
/// </summary>
public class KnowledgeGap
{
    public string Topic { get; set; } = string.Empty;
    public string SubTopic { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is not KnowledgeGap other) return false;
        
        return Topic == other.Topic &&
               SubTopic == other.SubTopic &&
               Severity == other.Severity &&
               Description == other.Description;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Topic, SubTopic, Severity, Description);
    }
}
