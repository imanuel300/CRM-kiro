namespace CandidacyManagement.Application.ExternalSubmissions;

/// <summary>
/// פקודת הגשת מועמדות ממערכת חיצונית
/// </summary>
public record ExternalSubmissionCommand(
    // פרטי איש קשר
    string IdNumber,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,

    // פרטי מועמדות
    int CallForCandidatesId,

    // מסמכים מצורפים (Base64)
    IReadOnlyList<AttachedDocumentDto>? Documents,

    // הצהרות ניגוד עניינים
    ConflictOfInterestDeclarationDto? ConflictOfInterest,

    // הצהרות קרבה משפחתית
    IReadOnlyList<FamilyRelationDeclarationDto>? FamilyRelations);

/// <summary>
/// מסמך מצורף בקידוד Base64
/// </summary>
public record AttachedDocumentDto(
    string DocumentType,
    string FileName,
    string ContentType,
    string Base64Content);

/// <summary>
/// הצהרת ניגוד עניינים
/// </summary>
public record ConflictOfInterestDeclarationDto(
    string QuestionnaireResponses,
    bool HasConflict);

/// <summary>
/// הצהרת קרבה משפחתית
/// </summary>
public record FamilyRelationDeclarationDto(
    string RelationType,
    string RelatedPersonName,
    string? RelatedPersonRole);
