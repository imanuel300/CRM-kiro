namespace CandidacyManagement.Application.Interviews;

public interface IInterviewService
{
    // Interview CRUD
    Task<InterviewDto> CreateAsync(CreateInterviewCommand command, CancellationToken cancellationToken = default);
    Task<InterviewDto> UpdateAsync(UpdateInterviewCommand command, CancellationToken cancellationToken = default);
    Task<InterviewDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewDto>> ListAsync(InterviewQueryParams query, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Feedback
    Task<InterviewFeedbackDto> SubmitFeedbackAsync(SubmitFeedbackCommand command, CancellationToken cancellationToken = default);
    Task<IEnumerable<InterviewFeedbackDto>> GetFeedbackAsync(int interviewId, CancellationToken cancellationToken = default);

    // Second Interview
    Task<InterviewDto> ScheduleSecondInterviewAsync(int interviewId, CreateInterviewCommand command, CancellationToken cancellationToken = default);
}
