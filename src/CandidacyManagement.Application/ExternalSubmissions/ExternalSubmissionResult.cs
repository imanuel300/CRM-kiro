namespace CandidacyManagement.Application.ExternalSubmissions;

/// <summary>
/// תוצאת קליטת מועמדות - מוחזר למערכת החיצונית
/// </summary>
public record SubmissionResult(
    int CandidacyId,
    int ContactId,
    string Status,
    DateTime SubmittedAt,
    IReadOnlyList<int>? DocumentIds);

/// <summary>
/// תוצאת ולידציה - רשימת שגיאות לפי שדה
/// </summary>
public record SubmissionValidationResult(
    bool IsValid,
    IDictionary<string, string[]> Errors);
