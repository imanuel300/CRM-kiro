using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// מעבר סטטוס מותר - מגדיר מעבר חוקי בין שני סטטוסים ביחידה ארגונית
/// </summary>
public class StatusTransition : BaseEntity
{
    public int OrgUnitId { get; set; }
    public int FromStatusId { get; set; }
    public int ToStatusId { get; set; }
    public string? RequiredPermission { get; set; }
    public bool RequiresReason { get; set; }
    public string? AutoTriggerRule { get; set; }

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public StatusDefinition FromStatus { get; set; } = null!;
    public StatusDefinition ToStatus { get; set; } = null!;
}
