using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// איש קשר - ישות המייצגת אדם במערכת (מועמד, חבר ועדה, שופט וכו')
/// </summary>
public class Contact : AuditableEntity
{
    public string IdNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Navigation properties
    public ICollection<Candidacy> Candidacies { get; set; } = new List<Candidacy>();
    public ICollection<ContactCustomFieldValue> CustomFieldValues { get; set; } = new List<ContactCustomFieldValue>();
    public ICollection<ContactChangeHistory> ChangeHistory { get; set; } = new List<ContactChangeHistory>();
}
