using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Interviews;

public record InterviewDto(
    int Id,
    int OrgUnitId,
    int CallForCandidatesId,
    int CandidacyId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string? Location,
    List<int> InterviewerIds,
    InterviewType InterviewType,
    InterviewStatus Status,
    DateTime CreatedAt);

public record InterviewFeedbackDto(
    int Id,
    int InterviewId,
    int InterviewerId,
    decimal Rating,
    string? Comments,
    DateTime SubmittedAt);

public record CreateInterviewCommand(
    int OrgUnitId,
    int CallForCandidatesId,
    int CandidacyId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string? Location,
    List<int> InterviewerIds,
    InterviewType InterviewType);

public record UpdateInterviewCommand(
    int Id,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string? Location,
    List<int> InterviewerIds);

public record SubmitFeedbackCommand(
    int InterviewId,
    int InterviewerId,
    decimal Rating,
    string? Comments);

public record InterviewQueryParams(
    int? OrgUnitId = null,
    int? CallForCandidatesId = null,
    int? CandidacyId = null);
