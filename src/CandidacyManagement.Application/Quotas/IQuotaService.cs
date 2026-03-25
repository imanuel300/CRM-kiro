namespace CandidacyManagement.Application.Quotas;

public interface IQuotaService
{
    Task<QuotaDto> CreateAsync(CreateQuotaCommand command, CancellationToken cancellationToken = default);
    Task<QuotaDto> UpdateAsync(UpdateQuotaCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<QuotaDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuotaDto>> GetByOrgUnitAsync(int orgUnitId, CancellationToken cancellationToken = default);
    Task<QuotaAssignmentDto> AssignCandidacyAsync(AssignCandidacyCommand command, CancellationToken cancellationToken = default);
    Task UnassignCandidacyAsync(UnassignCandidacyCommand command, CancellationToken cancellationToken = default);
    Task<OrgUnitFulfillmentDto> GetFulfillmentStatusAsync(int orgUnitId, CancellationToken cancellationToken = default);
}
