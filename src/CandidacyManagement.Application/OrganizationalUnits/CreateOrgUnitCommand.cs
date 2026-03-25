namespace CandidacyManagement.Application.OrganizationalUnits;

public record CreateOrgUnitCommand(
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone);
