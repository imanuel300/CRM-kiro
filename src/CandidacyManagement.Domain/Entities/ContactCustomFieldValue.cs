using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// ערך שדה מותאם אישית לאיש קשר - ברמת יחידה ארגונית
/// </summary>
public class ContactCustomFieldValue : BaseEntity
{
    public int ContactId { get; set; }
    public int CustomFieldDefinitionId { get; set; }
    public int OrgUnitId { get; set; }
    public string? Value { get; set; }

    // Navigation properties
    public Contact Contact { get; set; } = null!;
    public CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
    public OrganizationalUnit OrgUnit { get; set; } = null!;
}
