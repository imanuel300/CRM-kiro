using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// כהונה - תקופת שירות של נציג שהתקבל לתפקיד
/// </summary>
public class Tenure : AuditableEntity
{
    public int ContactId { get; set; }
    public int OrgUnitId { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public TenureEndReason? EndReason { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public Contact Contact { get; set; } = null!;
    public OrganizationalUnit OrgUnit { get; set; } = null!;
}
