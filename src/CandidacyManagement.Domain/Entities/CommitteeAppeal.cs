using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// ערעור על החלטת ועדה
/// </summary>
public class CommitteeAppeal : BaseEntity
{
    public int MeetingId { get; set; }
    public int CandidacyId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Result { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public CommitteeMeeting Meeting { get; set; } = null!;
    public Candidacy Candidacy { get; set; } = null!;
}
