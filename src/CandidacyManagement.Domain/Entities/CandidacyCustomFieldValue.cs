using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// ערך שדה מותאם אישית למועמדות - ברמת יחידה ארגונית
/// </summary>
public class CandidacyCustomFieldValue : BaseEntity
{
    public int CandidacyId { get; set; }
    public int CustomFieldDefinitionId { get; set; }
    public string? Value { get; set; }

    // Navigation properties
    public Candidacy Candidacy { get; set; } = null!;
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
}
