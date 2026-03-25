using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// החלטת ועדה - החלטה על מועמדות שנדונה בישיבת ועדה
/// </summary>
public class CommitteeDecision : BaseEntity
{
    public int MeetingId { get; set; }
    public int CandidacyId { get; set; }
    public CommitteeDecisionType Decision { get; set; }
    public string? Recommendation { get; set; }
    public int DecidedBy { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public CommitteeMeeting Meeting { get; set; } = null!;
    public Candidacy Candidacy { get; set; } = null!;
}
