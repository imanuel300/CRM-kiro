namespace CandidacyManagement.Application.Committees;

public interface ICommitteeService
{
    // Committee CRUD
    Task<CommitteeDto> CreateAsync(CreateCommitteeCommand command, CancellationToken cancellationToken = default);
    Task<CommitteeDto> UpdateAsync(UpdateCommitteeCommand command, CancellationToken cancellationToken = default);
    Task<CommitteeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CommitteeDto>> ListAsync(CommitteeQueryParams query, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Meetings
    Task<CommitteeMeetingDto> CreateMeetingAsync(CreateMeetingCommand command, CancellationToken cancellationToken = default);
    Task<CommitteeMeetingDto> GetMeetingAsync(int meetingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CommitteeMeetingDto>> ListMeetingsAsync(int committeeId, CancellationToken cancellationToken = default);

    // Decisions
    Task<CommitteeDecisionDto> RecordDecisionAsync(RecordDecisionCommand command, CancellationToken cancellationToken = default);
    Task<IEnumerable<CommitteeDecisionDto>> GetDecisionsAsync(int meetingId, CancellationToken cancellationToken = default);

    // Appeals
    Task<CommitteeAppealDto> SubmitAppealAsync(SubmitCommitteeAppealCommand command, CancellationToken cancellationToken = default);
    Task<CommitteeAppealDto> ResolveAppealAsync(int appealId, string result, CancellationToken cancellationToken = default);

    // Protocol
    Task<string> GenerateProtocolAsync(int meetingId, CancellationToken cancellationToken = default);
}
