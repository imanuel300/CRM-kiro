using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// הרשאה - מגדירה סוג פעולה מותרת עבור תפקיד
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>מזהה התפקיד</summary>
    public int RoleId { get; set; }

    /// <summary>סוג ההרשאה</summary>
    public PermissionType PermissionType { get; set; }

    // Navigation properties
    public Role? Role { get; set; }
}
