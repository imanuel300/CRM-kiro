namespace CandidacyManagement.Application.CallsForCandidates;

public interface ICallForCandidatesService
{
    Task<CallForCandidatesDto> CreateAsync(CreateCallForCandidatesCommand command, CancellationToken cancellationToken = default);
    Task<CallForCandidatesDto> UpdateAsync(UpdateCallForCandidatesCommand command, CancellationToken cancellationToken = default);
    Task<CallForCandidatesDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CallForCandidatesDetailDto> GetDetailAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CallForCandidatesDto>> ListAsync(CallForCandidatesQueryParams query, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ThresholdConditionDto> AddThresholdConditionAsync(CreateThresholdConditionCommand command, CancellationToken cancellationToken = default);
    Task RemoveThresholdConditionAsync(int conditionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ThresholdConditionDto>> GetThresholdConditionsAsync(int callForCandidatesId, CancellationToken cancellationToken = default);
    Task<PositionDto> AddPositionAsync(CreatePositionCommand command, CancellationToken cancellationToken = default);
    Task RemovePositionAsync(int positionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PositionDto>> GetPositionsAsync(int callForCandidatesId, CancellationToken cancellationToken = default);
    Task<bool> IsClosedAsync(int callForCandidatesId, CancellationToken cancellationToken = default);
    Task<ClosingSummaryDto> GetClosingSummaryAsync(int callForCandidatesId, CancellationToken cancellationToken = default);
}
