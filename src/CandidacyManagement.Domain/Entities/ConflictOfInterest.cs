using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// הצהרת ניגוד עניינים - שאלון שמועמד ממלא במסגרת הגשת מועמדות
/// </summary>
public class ConflictOfInterest : BaseEntity
{
    public int CandidacyId { get; set; }
    public int ContactId { get; set; }
    public string QuestionnaireResponses { get; set; } = string.Empty;
    public bool HasConflict { get; set; }
    public bool RequiresManualReview { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public Candidacy Candidacy { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
}
