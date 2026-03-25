using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// תפקיד משתמש - מגדיר קבוצת הרשאות ברמת יחידה ארגונית
/// </summary>
public class Role : AuditableEntity
{
    /// <summary>שם התפקיד</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>תיאור התפקיד</summary>
    public string? Description { get; set; }

    /// <summary>מזהה יחידה ארגונית</summary>
    public int OrgUnitId { get; set; }

    /// <summary>האם מאפשר גישה חוצת-יחידות</summary>
    public bool AllowCrossUnit { get; set; }

    // Navigation properties
    public OrganizationalUnit? OrgUnit { get; set; }
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
