using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Security;

/// <summary>
/// שירות אבטחת מידע - מימוש אימות, MFA, ניהול סשן, נעילת חשבון, יומן גישה ומחיקת מידע אישי
/// </summary>
public class SecurityService : ISecurityService
{
    internal const int MaxFailedAttempts = 5;
    internal const int SessionTimeoutMinutes = 30;

    private readonly IRepository<UserAccount> _userAccountRepository;
    private readonly IRepository<PersonalDataAccessLog> _accessLogRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<AuditLogEntry> _auditLogRepository;

    public SecurityService(
        IRepository<UserAccount> userAccountRepository,
        IRepository<PersonalDataAccessLog> accessLogRepository,
        IRepository<Contact> contactRepository,
        IRepository<AuditLogEntry> auditLogRepository)
    {
        _userAccountRepository = userAccountRepository;
        _accessLogRepository = accessLogRepository;
        _contactRepository = contactRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return new LoginResult(false, false, null, null, "שם משתמש וסיסמה הם שדות חובה");

        var accounts = await _userAccountRepository.FindAsync(
            a => a.Username == command.Username, cancellationToken);
        var account = accounts.FirstOrDefault();

        if (account == null)
            return new LoginResult(false, false, null, null, "שם משתמש או סיסמה שגויים");

        if (account.IsLocked)
            return new LoginResult(false, false, null, null, "החשבון נעול. פנה למנהל המערכת");

        // Verify password
        if (!VerifyPassword(command.Password, account.PasswordHash))
        {
            account.FailedLoginAttempts++;

            if (account.FailedLoginAttempts >= MaxFailedAttempts)
            {
                account.IsLocked = true;
                account.LockedAt = DateTime.UtcNow;
                await _userAccountRepository.UpdateAsync(account, cancellationToken);

                await _auditLogRepository.AddAsync(new AuditLogEntry
                {
                    UserId = account.Id,
                    Action = "AccountLocked",
                    EntityType = "UserAccount",
                    EntityId = account.Id,
                    OrgUnitId = account.OrgUnitId,
                    Details = $"חשבון ננעל לאחר {MaxFailedAttempts} ניסיונות כושלים",
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);

                return new LoginResult(false, false, null, null, "החשבון ננעל לאחר ניסיונות כושלים מרובים");
            }

            await _userAccountRepository.UpdateAsync(account, cancellationToken);
            return new LoginResult(false, false, null, null, "שם משתמש או סיסמה שגויים");
        }

        // Reset failed attempts on successful password
        account.FailedLoginAttempts = 0;

        // Check if MFA is required for admin users
        if (account.IsAdmin && account.MfaEnabled)
        {
            await _userAccountRepository.UpdateAsync(account, cancellationToken);
            return new LoginResult(true, true, account.Id, null, null);
        }

        // Set session
        account.LastActivityAt = DateTime.UtcNow;
        account.SessionExpiresAt = DateTime.UtcNow.AddMinutes(SessionTimeoutMinutes);
        await _userAccountRepository.UpdateAsync(account, cancellationToken);

        return new LoginResult(true, false, account.Id, $"session-{account.Id}-{Guid.NewGuid()}", null);
    }

    public async Task<MfaVerificationResult> VerifyMfaAsync(VerifyMfaCommand command, CancellationToken cancellationToken = default)
    {
        var account = await _userAccountRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), command.UserId);

        if (!account.MfaEnabled || string.IsNullOrEmpty(account.MfaSecret))
            return new MfaVerificationResult(false, null, "MFA לא מופעל עבור חשבון זה");

        if (!VerifyMfaCode(command.MfaCode, account.MfaSecret))
            return new MfaVerificationResult(false, null, "קוד MFA שגוי");

        account.LastActivityAt = DateTime.UtcNow;
        account.SessionExpiresAt = DateTime.UtcNow.AddMinutes(SessionTimeoutMinutes);
        await _userAccountRepository.UpdateAsync(account, cancellationToken);

        return new MfaVerificationResult(true, $"session-{account.Id}-{Guid.NewGuid()}", null);
    }

    public async Task<SessionCheckResult> CheckSessionAsync(int userId, CancellationToken cancellationToken = default)
    {
        var account = await _userAccountRepository.GetByIdAsync(userId, cancellationToken);
        if (account == null)
            return new SessionCheckResult(false, null);

        if (account.SessionExpiresAt == null || account.SessionExpiresAt <= DateTime.UtcNow)
            return new SessionCheckResult(false, null);

        return new SessionCheckResult(true, account.SessionExpiresAt);
    }

    public async Task RefreshSessionAsync(int userId, CancellationToken cancellationToken = default)
    {
        var account = await _userAccountRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), userId);

        account.LastActivityAt = DateTime.UtcNow;
        account.SessionExpiresAt = DateTime.UtcNow.AddMinutes(SessionTimeoutMinutes);
        await _userAccountRepository.UpdateAsync(account, cancellationToken);
    }

    public async Task LogPersonalDataAccessAsync(
        int userId, int contactId, string accessType, string fieldsAccessed,
        string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var log = new PersonalDataAccessLog
        {
            UserId = userId,
            ContactId = contactId,
            AccessType = accessType,
            FieldsAccessed = fieldsAccessed,
            AccessedAt = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        await _accessLogRepository.AddAsync(log, cancellationToken);
    }

    public async Task ErasePersonalDataAsync(ErasePersonalDataCommand command, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.ContactId);

        // Anonymize personal data
        contact.FirstName = "[נמחק]";
        contact.LastName = "[נמחק]";
        contact.IdNumber = $"ERASED-{contact.Id}";
        contact.Phone = null;
        contact.Email = null;
        contact.Address = null;
        contact.DateOfBirth = null;
        contact.Gender = null;
        contact.UpdatedAt = DateTime.UtcNow;

        await _contactRepository.UpdateAsync(contact, cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLogEntry
        {
            UserId = command.RequestedByUserId,
            Action = "PersonalDataErased",
            EntityType = "Contact",
            EntityId = command.ContactId,
            Details = $"מידע אישי נמחק. סיבה: {command.Reason}",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task UnlockAccountAsync(UnlockAccountCommand command, CancellationToken cancellationToken = default)
    {
        var account = await _userAccountRepository.GetByIdAsync(command.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), command.UserAccountId);

        account.IsLocked = false;
        account.LockedAt = null;
        account.FailedLoginAttempts = 0;
        await _userAccountRepository.UpdateAsync(account, cancellationToken);

        await _auditLogRepository.AddAsync(new AuditLogEntry
        {
            UserId = command.UnlockedByUserId,
            Action = "AccountUnlocked",
            EntityType = "UserAccount",
            EntityId = command.UserAccountId,
            Details = "חשבון שוחרר מנעילה על ידי מנהל",
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
    }

    // --- Private helpers ---

    internal static bool VerifyPassword(string password, string passwordHash)
    {
        var hash = ComputeHash(password);
        return string.Equals(hash, passwordHash, StringComparison.Ordinal);
    }

    internal static string ComputeHash(string input)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyMfaCode(string code, string secret)
    {
        // Simplified TOTP verification: in production use a proper TOTP library
        return !string.IsNullOrWhiteSpace(code) && code == secret;
    }
}
