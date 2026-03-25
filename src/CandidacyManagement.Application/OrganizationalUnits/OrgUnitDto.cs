namespace CandidacyManagement.Application.OrganizationalUnits;

public record OrgUnitDto(
    int Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
