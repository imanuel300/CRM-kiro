using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// תפקיד במבנה ארגוני - משרה ביחידת משנה עם מספר מקסימלי של מאיישים
/// </summary>
public class OrgPosition : AuditableEntity
{
    public int OrgSubUnitId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int MaxOccupants { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public OrgSubUnit OrgSubUnit { get; set; } = null!;
    public ICollection<PositionAssignment> Assignments { get; set; } = new List<PositionAssignment>();
}
