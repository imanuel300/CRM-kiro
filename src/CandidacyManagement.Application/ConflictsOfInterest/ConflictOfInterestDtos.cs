namespace CandidacyManagement.Application.ConflictsOfInterest;

/// <summary>
/// DTO להצהרת ניגוד עניינים
/// </summary>
public record ConflictOfInterestDto(
    int Id,
    int CandidacyId,
    int ContactId,
    string QuestionnaireResponses,
    bool HasConflict,
    bool RequiresManualReview,
    int? ReviewedByUserId,
    DateTime? ReviewedAt);

/// <summary>
/// DTO להצהרת קרבה משפחתית
/// </summary>
public record FamilyRelationDto(
    int Id,
    int CandidacyId,
    int ContactId,
    string RelationType,
    string RelatedPersonName,
    string? RelatedPersonRole,
    bool RequiresManualReview);

/// <summary>
/// כל ההצהרות של מועמדות - ניגוד עניינים וקרבה משפחתית
/// </summary>
public record CandidacyDeclarationsDto(
    int CandidacyId,
    IReadOnlyList<ConflictOfInterestDto> ConflictsOfInterest,
    IReadOnlyList<FamilyRelationDto> FamilyRelations);

/// <summary>
/// פקודת יצירת הצהרת ניגוד עניינים
/// </summary>
public record CreateConflictOfInterestCommand(
    int CandidacyId,
    int ContactId,
    string QuestionnaireResponses,
    bool HasConflict);

/// <summary>
/// פקודת עדכון הצהרת ניגוד עניינים
/// </summary>
public record UpdateConflictOfInterestCommand(
    int Id,
    string QuestionnaireResponses,
    bool HasConflict);

/// <summary>
/// פקודת יצירת הצהרת קרבה משפחתית
/// </summary>
public record CreateFamilyRelationCommand(
    int CandidacyId,
    int ContactId,
    string RelationType,
    string RelatedPersonName,
    string? RelatedPersonRole);

/// <summary>
/// פקודת עדכון הצהרת קרבה משפחתית
/// </summary>
public record UpdateFamilyRelationCommand(
    int Id,
    string RelationType,
    string RelatedPersonName,
    string? RelatedPersonRole);

/// <summary>
/// פקודת סקירת ניגוד עניינים על ידי משתמש מנהל
/// </summary>
public record ReviewConflictCommand(
    int Id,
    int ReviewedByUserId);

/// <summary>
/// פרמטרי שאילתה למועמדויות הדורשות בדיקה ידנית
/// </summary>
public record ManualReviewQueryParams(
    int? OrgUnitId = null);
