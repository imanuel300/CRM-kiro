namespace CandidacyManagement.Application.Tenures;

public interface ITenureService
{
    Task<TenureDto> CreateAsync(CreateTenureCommand command, CancellationToken cancellationToken = default);
    Task<TenureDto> UpdateAsync(UpdateTenureCommand command, CancellationToken cancellationToken = default);
    Task<TenureDto> EndTenureAsync(EndTenureCommand command, CancellationToken cancellationToken = default);
    Task<TenureDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenureDto>> GetByContactAsync(int contactId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenureDto>> GetByOrgUnitAsync(int orgUnitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExpiringTenureDto>> GetExpiringTenuresAsync(int daysThreshold, int? orgUnitId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenureDto>> GetHistoryAsync(int contactId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
