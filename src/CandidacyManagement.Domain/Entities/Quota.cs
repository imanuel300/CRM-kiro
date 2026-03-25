using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// מכסה - הגדרת יעד ייצוג הולם לפי קטגוריה ברמת יחידה ארגונית
/// </summary>
public class Quota : AuditableEntity
{
    public int OrgUnitId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TargetCount { get; set; }
    public int CurrentCount { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public ICollection<QuotaAssignment> Assignments { get; set; } = new List<QuotaAssignment>();
}
