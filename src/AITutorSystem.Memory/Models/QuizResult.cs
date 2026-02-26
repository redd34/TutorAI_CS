namespace AITutorSystem.Memory.Models;

/// <summary>
/// Represents the result of a completed quiz with analysis.
/// </summary>
public class QuizResult
{
    public string Id { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public Dictionary<string, string> Answers { get; set; } = new();
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public List<KnowledgeGap> IdentifiedGaps { get; set; } = new();
    public DateTime CompletedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not QuizResult other) return false;
        
        return Id == other.Id &&
               QuizId == other.QuizId &&
               StudentId == other.StudentId &&
               Answers.Count == other.Answers.Count &&
               Answers.All(kvp => other.Answers.TryGetValue(kvp.Key, out var value) && value == kvp.Value) &&
               Score == other.Score &&
               TotalQuestions == other.TotalQuestions &&
               Passed == other.Passed &&
               IdentifiedGaps.SequenceEqual(other.IdentifiedGaps) &&
               CompletedAt == other.CompletedAt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, QuizId, StudentId, Score, TotalQuestions, Passed, CompletedAt);
    }
}
