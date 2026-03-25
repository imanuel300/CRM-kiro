namespace CandidacyManagement.Application.Documents;

public record DocumentDto(
    int Id,
    int CandidacyId,
    string DocumentType,
    string FileName,
    string BlobUrl,
    string ContentType,
    long SizeBytes,
    string Status,
    int? ReviewedByUserId,
    DateTime UploadedAt,
    int Version);

public record DocumentVersionDto(
    int Id,
    string FileName,
    string BlobUrl,
    long SizeBytes,
    string Status,
    DateTime UploadedAt,
    int Version);

public record RequiredDocumentDto(
    int Id,
    int? CallForCandidatesId,
    int? OrgUnitId,
    string DocumentType,
    bool IsRequired,
    string AllowedFormats,
    int MaxSizeKB);

public record UploadDocumentCommand(
    int CandidacyId,
    string DocumentType,
    string FileName,
    string BlobUrl,
    string ContentType,
    long SizeBytes);

public record ReviewDocumentCommand(
    int DocumentId,
    string Status, // "Approved" or "Rejected"
    int ReviewedByUserId);

public record CreateRequiredDocumentCommand(
    int? CallForCandidatesId,
    int? OrgUnitId,
    string DocumentType,
    bool IsRequired,
    string AllowedFormats,
    int MaxSizeKB);

public record DocumentCompletenessResult(
    bool IsComplete,
    IEnumerable<MissingDocumentInfo> MissingDocuments);

public record MissingDocumentInfo(
    string DocumentType,
    bool IsRequired);

public record DocumentQueryParams(
    int? CandidacyId = null,
    string? DocumentType = null,
    string? Status = null);
