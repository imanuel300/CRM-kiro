namespace CandidacyManagement.Application.Security;

/// <summary>
/// ממשק שירות אבטחת מידע - אימות, MFA, ניהול סשן, נעילת חשבון, יומן גישה ומחיקת מידע אישי
/// דרישות: 18.1-18.6
/// </summary>
public interface ISecurityService
{
    /// <summary>התחברות עם נעילת חשבון לאחר 5 ניסיונות כושלים</summary>
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);

    /// <summary>אימות MFA למשתמשים מנהלים</summary>
    Task<MfaVerificationResult> VerifyMfaAsync(VerifyMfaCommand command, CancellationToken cancellationToken = default);

    /// <summary>בדיקת תקינות סשן (30 דקות timeout)</summary>
    Task<SessionCheckResult> CheckSessionAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>רענון פעילות סשן</summary>
    Task RefreshSessionAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>תיעוד גישה למידע אישי</summary>
    Task LogPersonalDataAccessAsync(int userId, int contactId, string accessType, string fieldsAccessed, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>מחיקת מידע אישי - Right to Erasure (דרישה 18.6)</summary>
    Task ErasePersonalDataAsync(ErasePersonalDataCommand command, CancellationToken cancellationToken = default);

    /// <summary>שחרור נעילת חשבון</summary>
    Task UnlockAccountAsync(UnlockAccountCommand command, CancellationToken cancellationToken = default);
}
