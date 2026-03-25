using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// יחידת משנה ארגונית - חלק ממבנה היררכי של יחידה ארגונית (לשכה, מחוז וכו')
/// </summary>
public class OrgSubUnit : AuditableEntity
{
    public int OrgUnitId { get; set; }
    public int? ParentOrgSubUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public OrgSubUnit? Parent { get; set; }
    public ICollection<OrgSubUnit> Children { get; set; } = new List<OrgSubUnit>();
    public ICollection<OrgPosition> Positions { get; set; } = new List<OrgPosition>();
}
