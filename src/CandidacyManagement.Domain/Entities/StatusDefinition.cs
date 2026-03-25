using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// הגדרת סטטוס מועמדות - סטטוס ייחודי ליחידה ארגונית
/// </summary>
public class StatusDefinition : BaseEntity
{
    public int OrgUnitId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public CandidacyStatusCategory Category { get; set; }
    public bool IsFinal { get; set; }
    public bool IsInitial { get; set; }
    public int SortOrder { get; set; }

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public ICollection<SubStatusDefinition> SubStatuses { get; set; } = new List<SubStatusDefinition>();
}
