namespace CandidacyManagement.Application.Candidacies;

public interface ICandidacyService
{
    Task<CandidacyDto> CreateAsync(CreateCandidacyCommand command, CancellationToken cancellationToken = default);
    Task<CandidacyDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CandidacyDetailDto> GetDetailAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CandidacyDto>> ListAsync(CandidacyQueryParams query, CancellationToken cancellationToken = default);
    Task<CandidacyDto> UpdateAsync(UpdateCandidacyCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CandidacyCustomFieldValueDto>> GetCustomFieldsAsync(int candidacyId, CancellationToken cancellationToken = default);
    Task SetCustomFieldValueAsync(SetCandidacyCustomFieldValueCommand command, CancellationToken cancellationToken = default);
    Task<CandidacyDto> TransitionStatusAsync(TransitionStatusCommand command, CancellationToken cancellationToken = default);
    Task<IEnumerable<StatusHistoryDto>> GetStatusHistoryAsync(int candidacyId, CancellationToken cancellationToken = default);
}
