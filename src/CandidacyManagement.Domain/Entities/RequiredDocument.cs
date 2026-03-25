using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// מסמך נדרש - הגדרת סוג מסמך נדרש או אופציונלי ברמת יחידה ארגונית או קול קורא
/// </summary>
public class RequiredDocument : BaseEntity
{
    public int? CallForCandidatesId { get; set; }
    public int? OrgUnitId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public string AllowedFormats { get; set; } = string.Empty; // comma-separated, e.g. "pdf,docx,jpg"
    public int MaxSizeKB { get; set; } = 10240; // default 10MB

    // Navigation properties
    public CallForCandidates? CallForCandidates { get; set; }
    public OrganizationalUnit? OrgUnit { get; set; }
}
