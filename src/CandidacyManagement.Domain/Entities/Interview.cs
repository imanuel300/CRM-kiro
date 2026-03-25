using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// ראיון - שלב מיון הכולל פגישה עם המועמד, לוח זמנים ומשוב
/// </summary>
public class Interview : BaseEntity
{
    public int OrgUnitId { get; set; }
    public int CallForCandidatesId { get; set; }
    public int CandidacyId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Location { get; set; }
    public InterviewType InterviewType { get; set; } = InterviewType.First;
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    /// <summary>
    /// רשימת מזהי מראיינים (JSON-serialized)
    /// </summary>
    public string InterviewerIdsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public CallForCandidates CallForCandidates { get; set; } = null!;
    public Candidacy Candidacy { get; set; } = null!;
    public ICollection<InterviewFeedback> Feedbacks { get; set; } = new List<InterviewFeedback>();
}
