namespace CandidacyManagement.Application.OrgStructure;

public interface IOrgStructureService
{
    // Sub-Unit operations
    Task<OrgSubUnitDto> CreateSubUnitAsync(CreateSubUnitCommand command, CancellationToken cancellationToken = default);
    Task<OrgSubUnitDto> UpdateSubUnitAsync(UpdateSubUnitCommand command, CancellationToken cancellationToken = default);
    Task DeleteSubUnitAsync(int id, CancellationToken cancellationToken = default);
    Task<OrgSubUnitTreeDto> GetSubUnitTreeAsync(int orgUnitId, CancellationToken cancellationToken = default);

    // Position operations
    Task<OrgPositionDto> CreatePositionAsync(CreatePositionCommand command, CancellationToken cancellationToken = default);
    Task<OrgPositionDto> UpdatePositionAsync(UpdatePositionCommand command, CancellationToken cancellationToken = default);
    Task DeletePositionAsync(int id, CancellationToken cancellationToken = default);

    // Assignment operations
    Task<PositionAssignmentDto> AssignToPositionAsync(AssignToPositionCommand command, CancellationToken cancellationToken = default);
    Task UnassignFromPositionAsync(UnassignFromPositionCommand command, CancellationToken cancellationToken = default);

    // Occupancy
    Task<SubUnitOccupancyDto> GetPositionOccupancyAsync(int subUnitId, CancellationToken cancellationToken = default);
}
