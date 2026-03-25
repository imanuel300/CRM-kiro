using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// הגדרת תהליך מיון - מגדיר את שלבי המיון הפעילים ליחידה ארגונית
/// </summary>
public class WorkflowDefinition : BaseEntity
{
    public int OrgUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool ExamStepEnabled { get; set; }
    public bool InterviewStepEnabled { get; set; }
    public bool CommitteeStepEnabled { get; set; }
    public bool ThresholdCheckEnabled { get; set; }
    public string? StepOrder { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
}
