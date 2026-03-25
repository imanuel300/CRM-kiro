namespace CandidacyManagement.Application.OrgStructure;

// Sub-Unit DTOs
public record OrgSubUnitDto(
    int Id,
    int OrgUnitId,
    int? ParentOrgSubUnitId,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);

public record OrgSubUnitTreeDto(
    int Id,
    int OrgUnitId,
    int? ParentOrgSubUnitId,
    string Name,
    string? Description,
    bool IsActive,
    IEnumerable<OrgSubUnitTreeDto> Children,
    IEnumerable<OrgPositionDto> Positions);

public record CreateSubUnitCommand(
    int OrgUnitId,
    int? ParentOrgSubUnitId,
    string Name,
    string? Description);

public record UpdateSubUnitCommand(
    int Id,
    string Name,
    string? Description,
    bool IsActive);

// Position DTOs
public record OrgPositionDto(
    int Id,
    int OrgSubUnitId,
    string Title,
    int MaxOccupants,
    bool IsActive,
    DateTime CreatedAt);

public record CreatePositionCommand(
    int OrgSubUnitId,
    string Title,
    int MaxOccupants);

public record UpdatePositionCommand(
    int Id,
    string Title,
    int MaxOccupants,
    bool IsActive);

// Assignment DTOs
public record AssignToPositionCommand(
    int OrgPositionId,
    int ContactId,
    int CandidacyId,
    DateTime StartDate);

public record UnassignFromPositionCommand(int AssignmentId);

public record PositionAssignmentDto(
    int Id,
    int OrgPositionId,
    int ContactId,
    int CandidacyId,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    DateTime CreatedAt);

// Occupancy DTOs
public record PositionOccupancyDto(
    int PositionId,
    string Title,
    int MaxOccupants,
    int FilledCount,
    int VacantCount);

public record SubUnitOccupancyDto(
    int SubUnitId,
    string SubUnitName,
    IEnumerable<PositionOccupancyDto> Positions);
