using System.Text.Json;
using CandidacyManagement.Application.Calendar;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Interviews;

public class InterviewService : IInterviewService
{
    private readonly IRepository<Interview> _interviewRepository;
    private readonly IRepository<InterviewFeedback> _feedbackRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<CallForCandidates> _callRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IRepository<StatusDefinition> _statusRepository;
    private readonly IRepository<StatusTransition> _transitionRepository;
    private readonly IRepository<CandidacyStatusHistory> _historyRepository;
    private readonly ICalendarService _calendarService;

    public InterviewService(
        IRepository<Interview> interviewRepository,
        IRepository<InterviewFeedback> feedbackRepository,
        IRepository<Candidacy> candidacyRepository,
        IRepository<CallForCandidates> callRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IRepository<StatusDefinition> statusRepository,
        IRepository<StatusTransition> transitionRepository,
        IRepository<CandidacyStatusHistory> historyRepository,
        ICalendarService calendarService)
    {
        _interviewRepository = interviewRepository;
        _feedbackRepository = feedbackRepository;
        _candidacyRepository = candidacyRepository;
        _callRepository = callRepository;
        _orgUnitRepository = orgUnitRepository;
        _statusRepository = statusRepository;
        _transitionRepository = transitionRepository;
        _historyRepository = historyRepository;
        _calendarService = calendarService;
    }

    // --- Interview CRUD ---

    public async Task<InterviewDto> CreateAsync(CreateInterviewCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        _ = await _callRepository.GetByIdAsync(command.CallForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), command.CallForCandidatesId);

        _ = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        if (command.InterviewerIds == null || command.InterviewerIds.Count == 0)
            throw new ValidationException("InterviewerIds", "יש לציין לפחות מראיין אחד");

        if (command.EndTime <= command.StartTime)
            throw new ValidationException("EndTime", "שעת סיום חייבת להיות אחרי שעת התחלה");

        var entity = new Interview
        {
            OrgUnitId = command.OrgUnitId,
            CallForCandidatesId = command.CallForCandidatesId,
            CandidacyId = command.CandidacyId,
            ScheduledDate = command.ScheduledDate,
            StartTime = command.StartTime,
            EndTime = command.EndTime,
            Location = command.Location,
            InterviewType = command.InterviewType,
            Status = InterviewStatus.Scheduled,
            InterviewerIdsJson = JsonSerializer.Serialize(command.InterviewerIds)
        };

        await _interviewRepository.AddAsync(entity, cancellationToken);

        // Send calendar invite to interviewers
        await _calendarService.SendInterviewInviteAsync(entity, command.InterviewerIds, cancellationToken);

        return ToDto(entity);
    }

    public async Task<InterviewDto> UpdateAsync(UpdateInterviewCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _interviewRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Interview), command.Id);

        if (entity.Status == InterviewStatus.Completed)
            throw new ValidationException("Status", "לא ניתן לעדכן ראיון שהושלם");

        if (command.InterviewerIds == null || command.InterviewerIds.Count == 0)
            throw new ValidationException("InterviewerIds", "יש לציין לפחות מראיין אחד");

        if (command.EndTime <= command.StartTime)
            throw new ValidationException("EndTime", "שעת סיום חייבת להיות אחרי שעת התחלה");

        entity.ScheduledDate = command.ScheduledDate;
        entity.StartTime = command.StartTime;
        entity.EndTime = command.EndTime;
        entity.Location = command.Location;
        entity.InterviewerIdsJson = JsonSerializer.Serialize(command.InterviewerIds);

        await _interviewRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<InterviewDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _interviewRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Interview), id);

        return ToDto(entity);
    }

    public async Task<IEnumerable<InterviewDto>> ListAsync(InterviewQueryParams query, CancellationToken cancellationToken = default)
    {
        var results = await _interviewRepository.FindAsync(i =>
            (!query.OrgUnitId.HasValue || i.OrgUnitId == query.OrgUnitId.Value) &&
            (!query.CallForCandidatesId.HasValue || i.CallForCandidatesId == query.CallForCandidatesId.Value) &&
            (!query.CandidacyId.HasValue || i.CandidacyId == query.CandidacyId.Value),
            cancellationToken);

        return results.Select(ToDto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _interviewRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Interview), id);

        await _interviewRepository.DeleteAsync(entity, cancellationToken);
    }

    // --- Feedback ---

    public async Task<InterviewFeedbackDto> SubmitFeedbackAsync(SubmitFeedbackCommand command, CancellationToken cancellationToken = default)
    {
        var interview = await _interviewRepository.GetByIdAsync(command.InterviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(Interview), command.InterviewId);

        var interviewerIds = GetInterviewerIds(interview);

        if (!interviewerIds.Contains(command.InterviewerId))
            throw new ValidationException("InterviewerId", "המראיין אינו משויך לראיון זה");

        if (command.Rating < 0 || command.Rating > 10)
            throw new ValidationException("Rating", "דירוג חייב להיות בין 0 ל-10");

        // Check if this interviewer already submitted feedback
        var existingFeedbacks = await _feedbackRepository.FindAsync(
            f => f.InterviewId == command.InterviewId && f.InterviewerId == command.InterviewerId,
            cancellationToken);

        if (existingFeedbacks.Any())
            throw new ValidationException("InterviewerId", "המראיין כבר הגיש משוב לראיון זה");

        var feedback = new InterviewFeedback
        {
            InterviewId = command.InterviewId,
            InterviewerId = command.InterviewerId,
            Rating = command.Rating,
            Comments = command.Comments,
            SubmittedAt = DateTime.UtcNow
        };

        await _feedbackRepository.AddAsync(feedback, cancellationToken);

        // Check if all interviewers have submitted feedback
        await CheckAndUpdateCandidacyStatusAsync(interview, cancellationToken);

        return ToFeedbackDto(feedback);
    }

    public async Task<IEnumerable<InterviewFeedbackDto>> GetFeedbackAsync(int interviewId, CancellationToken cancellationToken = default)
    {
        _ = await _interviewRepository.GetByIdAsync(interviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(Interview), interviewId);

        var feedbacks = await _feedbackRepository.FindAsync(
            f => f.InterviewId == interviewId, cancellationToken);

        return feedbacks.Select(ToFeedbackDto);
    }

    // --- Second Interview ---

    public async Task<InterviewDto> ScheduleSecondInterviewAsync(int interviewId, CreateInterviewCommand command, CancellationToken cancellationToken = default)
    {
        var firstInterview = await _interviewRepository.GetByIdAsync(interviewId, cancellationToken)
            ?? throw new NotFoundException(nameof(Interview), interviewId);

        if (firstInterview.InterviewType != InterviewType.First)
            throw new ValidationException("InterviewType", "ניתן לתזמן ראיון שני רק מראיון ראשון");

        // Create the second interview linked to the same candidacy
        var secondCommand = command with
        {
            OrgUnitId = firstInterview.OrgUnitId,
            CallForCandidatesId = firstInterview.CallForCandidatesId,
            CandidacyId = firstInterview.CandidacyId,
            InterviewType = InterviewType.Second
        };

        return await CreateAsync(secondCommand, cancellationToken);
    }

    // --- Private helpers ---

    /// <summary>
    /// Checks if all interviewers have submitted feedback. If yes, marks the interview
    /// as completed and auto-updates the candidacy status.
    /// </summary>
    private async Task CheckAndUpdateCandidacyStatusAsync(Interview interview, CancellationToken cancellationToken)
    {
        var interviewerIds = GetInterviewerIds(interview);

        var allFeedbacks = await _feedbackRepository.FindAsync(
            f => f.InterviewId == interview.Id, cancellationToken);

        var submittedInterviewerIds = allFeedbacks.Select(f => f.InterviewerId).ToHashSet();

        if (!interviewerIds.All(id => submittedInterviewerIds.Contains(id)))
            return;

        // All interviewers have submitted - mark interview as completed
        interview.Status = InterviewStatus.Completed;
        await _interviewRepository.UpdateAsync(interview, cancellationToken);

        // Auto-update candidacy status
        var candidacy = await _candidacyRepository.GetByIdAsync(interview.CandidacyId, cancellationToken);
        if (candidacy == null || !candidacy.CurrentStatusId.HasValue || !candidacy.IsActive)
            return;

        // Calculate average rating
        var averageRating = allFeedbacks.Average(f => f.Rating);

        // Determine target status based on average rating (threshold: 5.0)
        var targetStatusCode = averageRating >= 5.0m ? "passed_interview" : "failed_interview";
        var targetStatusCodeHebrew = averageRating >= 5.0m ? "עבר_ראיון" : "נדחה_בראיון";

        var targetStatuses = await _statusRepository.FindAsync(
            s => s.OrgUnitId == candidacy.OrgUnitId
                 && (s.Code == targetStatusCode || s.Code == targetStatusCodeHebrew),
            cancellationToken);

        var targetStatus = targetStatuses.FirstOrDefault();
        if (targetStatus == null)
            return;

        // Verify the transition is allowed
        var transitions = await _transitionRepository.FindAsync(
            t => t.OrgUnitId == candidacy.OrgUnitId
                 && t.FromStatusId == candidacy.CurrentStatusId
                 && t.ToStatusId == targetStatus.Id,
            cancellationToken);

        if (!transitions.Any())
            return;

        // Record history
        var history = new CandidacyStatusHistory
        {
            CandidacyId = candidacy.Id,
            FromStatusId = candidacy.CurrentStatusId,
            ToStatusId = targetStatus.Id,
            Reason = $"עדכון אוטומטי - כל המראיינים הגישו משוב, דירוג ממוצע: {averageRating:F1}",
            ChangedAt = DateTime.UtcNow
        };

        candidacy.CurrentStatusId = targetStatus.Id;
        if (targetStatus.IsFinal)
            candidacy.IsActive = false;

        await _candidacyRepository.UpdateAsync(candidacy, cancellationToken);
        await _historyRepository.AddAsync(history, cancellationToken);
    }

    private static List<int> GetInterviewerIds(Interview interview)
    {
        try
        {
            return JsonSerializer.Deserialize<List<int>>(interview.InterviewerIdsJson) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }

    private static InterviewDto ToDto(Interview entity) =>
        new(entity.Id, entity.OrgUnitId, entity.CallForCandidatesId, entity.CandidacyId,
            entity.ScheduledDate, entity.StartTime, entity.EndTime, entity.Location,
            GetInterviewerIds(entity), entity.InterviewType, entity.Status, entity.CreatedAt);

    private static InterviewFeedbackDto ToFeedbackDto(InterviewFeedback entity) =>
        new(entity.Id, entity.InterviewId, entity.InterviewerId,
            entity.Rating, entity.Comments, entity.SubmittedAt);
}
