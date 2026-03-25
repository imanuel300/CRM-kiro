using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// חשבון משתמש - ניהול אימות, MFA, נעילת חשבון וניהול סשן
/// דרישות: 18.2, 18.3, 18.5
/// </summary>
public class UserAccount : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int FailedLoginAttempts { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime? SessionExpiresAt { get; set; }
    public bool IsAdmin { get; set; }
    public int? OrgUnitId { get; set; }
}
