using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Committees;

public record CommitteeMemberInfo(int MemberId, string Role);

public record CommitteeDto(
    int Id,
    int OrgUnitId,
    string Name,
    string? Description,
    List<CommitteeMemberInfo> Members,
    DateTime CreatedAt);

public record CommitteeMeetingDto(
    int Id,
    int CommitteeId,
    int OrgUnitId,
    DateTime ScheduledDate,
    string? Location,
    MeetingStatus Status,
    List<int> CandidacyIds,
    DateTime CreatedAt);

public record CommitteeDecisionDto(
    int Id,
    int MeetingId,
    int CandidacyId,
    CommitteeDecisionType Decision,
    string? Recommendation,
    int DecidedBy,
    DateTime DecidedAt);

public record CreateCommitteeCommand(
    int OrgUnitId,
    string Name,
    string? Description,
    List<CommitteeMemberInfo> Members);

public record UpdateCommitteeCommand(
    int Id,
    string Name,
    string? Description,
    List<CommitteeMemberInfo> Members);

public record CreateMeetingCommand(
    int CommitteeId,
    int OrgUnitId,
    DateTime ScheduledDate,
    string? Location,
    List<int> CandidacyIds);

public record RecordDecisionCommand(
    int MeetingId,
    int CandidacyId,
    CommitteeDecisionType Decision,
    string? Recommendation,
    int DecidedBy);

public record CommitteeQueryParams(
    int? OrgUnitId = null);

// --- Appeals ---

public record SubmitCommitteeAppealCommand(
    int MeetingId,
    int CandidacyId,
    string Reason);

public record ResolveCommitteeAppealCommand(
    int AppealId,
    string Result);

public record CommitteeAppealDto(
    int Id,
    int MeetingId,
    int CandidacyId,
    string Reason,
    string? Result,
    DateTime? ResolvedAt,
    DateTime CreatedAt);
