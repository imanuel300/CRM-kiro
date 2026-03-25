namespace CandidacyManagement.Application.OrganizationalUnits;

public interface IOrganizationalUnitService
{
    Task<OrgUnitDto> CreateAsync(CreateOrgUnitCommand command, CancellationToken cancellationToken = default);
    Task<OrgUnitDto> UpdateAsync(UpdateOrgUnitCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<OrgUnitDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrgUnitDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
