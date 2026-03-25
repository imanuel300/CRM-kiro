namespace CandidacyManagement.Application.Contacts;

public record ContactDto(
    int Id,
    string IdNumber,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? Phone,
    string? Email,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ContactDetailDto(
    int Id,
    string IdNumber,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? Phone,
    string? Email,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<CustomFieldValueDto> CustomFields,
    IEnumerable<ChangeHistoryDto> RecentChanges);

public record CustomFieldValueDto(
    int Id,
    int CustomFieldDefinitionId,
    string FieldName,
    string FieldType,
    string? Value,
    int OrgUnitId);

public record ChangeHistoryDto(
    int Id,
    string FieldName,
    string? OldValue,
    string? NewValue,
    int? ChangedByUserId,
    DateTime ChangedAt);

public record CreateContactCommand(
    string IdNumber,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? Phone,
    string? Email);

public record UpdateContactCommand(
    int Id,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? Phone,
    string? Email,
    int? ChangedByUserId);

public record SetCustomFieldValueCommand(
    int ContactId,
    int OrgUnitId,
    int CustomFieldDefinitionId,
    string? Value);
