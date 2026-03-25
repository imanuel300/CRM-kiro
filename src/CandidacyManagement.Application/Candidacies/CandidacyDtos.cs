namespace CandidacyManagement.Application.Candidacies;

public record CandidacyDto(
    int Id,
    int ContactId,
    int OrgUnitId,
    int CallForCandidatesId,
    int? CurrentStatusId,
    int? CurrentSubStatusId,
    int? WorkflowDefinitionVersion,
    bool IsActive,
    DateTime? SubmittedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CandidacyDetailDto(
    int Id,
    int ContactId,
    int OrgUnitId,
    int CallForCandidatesId,
    int? CurrentStatusId,
    int? CurrentSubStatusId,
    int? WorkflowDefinitionVersion,
    bool IsActive,
    DateTime? SubmittedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<CandidacyCustomFieldValueDto> CustomFields);

public record CandidacyCustomFieldValueDto(
    int Id,
    int CustomFieldDefinitionId,
    string FieldName,
    string FieldType,
    string? Value);

public record CreateCandidacyCommand(
    int ContactId,
    int OrgUnitId,
    int CallForCandidatesId);

public record UpdateCandidacyCommand(
    int Id,
    int? CurrentSubStatusId);

public record SetCandidacyCustomFieldValueCommand(
    int CandidacyId,
    int CustomFieldDefinitionId,
    string? Value);

public record CandidacyQueryParams(
    int? OrgUnitId = null,
    int? ContactId = null,
    int? CallForCandidatesId = null,
    bool? IsActive = null);

public record TransitionStatusCommand(
    int CandidacyId,
    int NewStatusId,
    string? Reason,
    int UserId);

public record StatusHistoryDto(
    int Id,
    int CandidacyId,
    int? FromStatusId,
    int ToStatusId,
    int? FromSubStatusId,
    int? ToSubStatusId,
    string? Reason,
    int ChangedByUserId,
    DateTime ChangedAt);
