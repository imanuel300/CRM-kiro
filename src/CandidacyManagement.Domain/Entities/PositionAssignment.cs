using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// שיוך מועמד לתפקיד - קישור בין מועמד שהתקבל לתפקיד ביחידת משנה
/// </summary>
public class PositionAssignment : AuditableEntity
{
    public int OrgPositionId { get; set; }
    public int ContactId { get; set; }
    public int CandidacyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public OrgPosition OrgPosition { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
    public Candidacy Candidacy { get; set; } = null!;
}
