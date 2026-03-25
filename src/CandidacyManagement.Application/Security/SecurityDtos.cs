namespace CandidacyManagement.Application.Security;

// --- Commands ---

public record LoginCommand(string Username, string Password);

public record VerifyMfaCommand(int UserId, string MfaCode);

public record ErasePersonalDataCommand(int ContactId, int RequestedByUserId, string Reason);

public record UnlockAccountCommand(int UserAccountId, int UnlockedByUserId);

// --- Results ---

public record LoginResult(
    bool Success,
    bool RequiresMfa,
    int? UserId,
    string? SessionToken,
    string? ErrorMessage);

public record MfaVerificationResult(
    bool Success,
    string? SessionToken,
    string? ErrorMessage);

public record SessionCheckResult(
    bool IsValid,
    DateTime? ExpiresAt);

public record PersonalDataAccessLogDto(
    int Id,
    int UserId,
    int ContactId,
    string AccessType,
    string FieldsAccessed,
    DateTime AccessedAt,
    string? IpAddress);
