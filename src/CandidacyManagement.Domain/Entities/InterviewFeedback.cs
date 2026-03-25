using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// משוב ראיון - דירוג והערות מכל מראיין
/// </summary>
public class InterviewFeedback : BaseEntity
{
    public int InterviewId { get; set; }
    public int InterviewerId { get; set; }
    public decimal Rating { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Interview Interview { get; set; } = null!;
}
