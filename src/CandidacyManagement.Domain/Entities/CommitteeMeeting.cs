using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// ישיבת ועדה - מפגש של ועדה לדיון והחלטה על מועמדויות
/// </summary>
public class CommitteeMeeting : BaseEntity
{
    public int CommitteeId { get; set; }
    public int OrgUnitId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Location { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;

    /// <summary>
    /// רשימת מזהי מועמדויות לדיון (JSON-serialized)
    /// </summary>
    public string CandidacyIdsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Committee Committee { get; set; } = null!;
    public ICollection<CommitteeDecision> Decisions { get; set; } = new List<CommitteeDecision>();
}
