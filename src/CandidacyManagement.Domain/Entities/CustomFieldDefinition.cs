using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// הגדרת שדה מותאם אישית - שדה דינמי ברמת יחידה ארגונית
/// </summary>
public class CustomFieldDefinition : BaseEntity
{
    public int OrgUnitId { get; set; }
    public string EntityType { get; set; } = string.Empty; // "Contact" or "Candidacy"
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty; // "Text", "Number", "Date", "Select"
    public bool IsRequired { get; set; }
    public string? ValidationRule { get; set; }
    public string? Options { get; set; } // JSON array for Select type
    public int SortOrder { get; set; }

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
}
