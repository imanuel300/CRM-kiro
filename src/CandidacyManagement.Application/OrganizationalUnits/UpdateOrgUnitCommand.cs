namespace CandidacyManagement.Application.OrganizationalUnits;

public record UpdateOrgUnitCommand(
    int Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone);
