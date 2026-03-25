using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// מועמדות - תהליך הגשה של איש קשר לתפקיד או קול קורא
/// </summary>
public class Candidacy : AuditableEntity
{
    public int ContactId { get; set; }
    public int OrgUnitId { get; set; }
    public int CallForCandidatesId { get; set; }
    public int? CurrentStatusId { get; set; }
    public int? CurrentSubStatusId { get; set; }
    public int? WorkflowDefinitionVersion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? SubmittedAt { get; set; }

    // Navigation properties
    public Contact Contact { get; set; } = null!;
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public CallForCandidates CallForCandidates { get; set; } = null!;
    public StatusDefinition? CurrentStatus { get; set; }
    public ICollection<CandidacyStatusHistory> StatusHistory { get; set; } = new List<CandidacyStatusHistory>();
    public ICollection<CandidacyCustomFieldValue> CustomFieldValues { get; set; } = new List<CandidacyCustomFieldValue>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<ThresholdCheckResult> ThresholdCheckResults { get; set; } = new List<ThresholdCheckResult>();
}
