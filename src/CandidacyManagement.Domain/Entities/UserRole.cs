using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// שיוך משתמש לתפקיד - מקשר בין משתמש לתפקיד ביחידה ארגונית
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>מזהה המשתמש</summary>
    public int UserId { get; set; }

    /// <summary>מזהה התפקיד</summary>
    public int RoleId { get; set; }

    /// <summary>מזהה יחידה ארגונית</summary>
    public int OrgUnitId { get; set; }

    /// <summary>תאריך שיוך</summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Role? Role { get; set; }
    public OrganizationalUnit? OrgUnit { get; set; }
}
