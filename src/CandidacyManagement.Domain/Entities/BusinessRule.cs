using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// כלל עסקי - לוגיקה מותנית המופעלת ברמת יחידה ארגונית
/// </summary>
public class BusinessRule : AuditableEntity
{
    public int OrgUnitId { get; set; }
    public BusinessRuleType RuleType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ConditionExpression { get; set; }
    public string? ActionType { get; set; }
    public string? ActionParameters { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
}
