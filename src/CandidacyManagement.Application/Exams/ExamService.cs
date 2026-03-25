using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Exams;

public class ExamService : IExamService
{
    private readonly IRepository<Exam> _examRepository;
    private readonly IRepository<ExamScore> _scoreRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<CallForCandidates> _callRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IRepository<BusinessRule> _businessRuleRepository;
    private readonly IRepository<StatusDefinition> _statusRepository;
    private readonly IRepository<StatusTransition> _transitionRepository;
    private readonly IRepository<CandidacyStatusHistory> _historyRepository;

    public ExamService(
        IRepository<Exam> examRepository,
        IRepository<ExamScore> scoreRepository,
        IRepository<Candidacy> candidacyRepository,
        IRepository<CallForCandidates> callRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IRepository<BusinessRule> businessRuleRepository,
        IRepository<StatusDefinition> statusRepository,
        IRepository<StatusTransition> transitionRepository,
        IRepository<CandidacyStatusHistory> historyRepository)
    {
        _examRepository = examRepository;
        _scoreRepository = scoreRepository;
        _candidacyRepository = candidacyRepository;
        _callRepository = callRepository;
        _orgUnitRepository = orgUnitRepository;
        _businessRuleRepository = businessRuleRepository;
        _statusRepository = statusRepository;
        _transitionRepository = transitionRepository;
        _historyRepository = historyRepository;
    }

    // --- Exam CRUD ---

    public async Task<ExamDto> CreateAsync(CreateExamCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        _ = await _callRepository.GetByIdAsync(command.CallForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), command.CallForCandidatesId);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "שם מבחן הוא שדה חובה");

        if (command.MaxScore <= 0)
            throw new ValidationException("MaxScore", "ציון מקסימלי חייב להיות גדול מאפס");

        var entity = new Exam
        {
            OrgUnitId = command.OrgUnitId,
            CallForCandidatesId = command.CallForCandidatesId,
            Name = command.Name.Trim(),
            ExamDate = command.ExamDate,
            Location = command.Location,
            MaxScore = command.MaxScore,
            PassingScore = command.PassingScore,
            FirstExaminerId = command.FirstExaminerId,
            SecondExaminerId = command.SecondExaminerId,
            AppealDeadline = command.AppealDeadline
        };

        await _examRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<ExamDto> UpdateAsync(UpdateExamCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _examRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Exam), command.Id);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "שם מבחן הוא שדה חובה");

        if (command.MaxScore <= 0)
            throw new ValidationException("MaxScore", "ציון מקסימלי חייב להיות גדול מאפס");

        entity.Name = command.Name.Trim();
        entity.ExamDate = command.ExamDate;
        entity.Location = command.Location;
        entity.MaxScore = command.MaxScore;
        entity.PassingScore = command.PassingScore;
        entity.FirstExaminerId = command.FirstExaminerId;
        entity.SecondExaminerId = command.SecondExaminerId;
        entity.AppealDeadline = command.AppealDeadline;

        await _examRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<ExamDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _examRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Exam), id);

        return ToDto(entity);
    }

    public async Task<IEnumerable<ExamDto>> ListAsync(ExamQueryParams query, CancellationToken cancellationToken = default)
    {
        var results = await _examRepository.FindAsync(e =>
            (!query.OrgUnitId.HasValue || e.OrgUnitId == query.OrgUnitId.Value) &&
            (!query.CallForCandidatesId.HasValue || e.CallForCandidatesId == query.CallForCandidatesId.Value),
            cancellationToken);

        return results.Select(ToDto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _examRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Exam), id);

        await _examRepository.DeleteAsync(entity, cancellationToken);
    }

    // --- Scores ---

    public async Task<ExamScoreDto> SubmitScoreAsync(SubmitScoreCommand command, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByIdAsync(command.ExamId, cancellationToken)
            ?? throw new NotFoundException(nameof(Exam), command.ExamId);

        var candidacy = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        // Validate scores are within range
        if (command.FirstExaminerScore.HasValue && (command.FirstExaminerScore.Value < 0 || command.FirstExaminerScore.Value > exam.MaxScore))
            throw new ValidationException("FirstExaminerScore", $"ציון בודק ראשון חייב להיות בין 0 ל-{exam.MaxScore}");

        if (command.SecondExaminerScore.HasValue && (command.SecondExaminerScore.Value < 0 || command.SecondExaminerScore.Value > exam.MaxScore))
            throw new ValidationException("SecondExaminerScore", $"ציון בודק שני חייב להיות בין 0 ל-{exam.MaxScore}");

        // Find existing score or create new
        var existingScores = await _scoreRepository.FindAsync(
            s => s.ExamId == command.ExamId && s.CandidacyId == command.CandidacyId,
            cancellationToken);

        var score = existingScores.FirstOrDefault();
        bool isNew = score == null;

        if (isNew)
        {
            score = new ExamScore
            {
                ExamId = command.ExamId,
                CandidacyId = command.CandidacyId
            };
        }

        if (command.FirstExaminerScore.HasValue)
            score!.FirstExaminerScore = command.FirstExaminerScore.Value;

        if (command.SecondExaminerScore.HasValue)
            score!.SecondExaminerScore = command.SecondExaminerScore.Value;

        // Calculate final score when both examiners have scored
        if (score!.FirstExaminerScore.HasValue && score.SecondExaminerScore.HasValue)
        {
            score.FinalScore = await CalculateFinalScore(
                score.FirstExaminerScore.Value,
                score.SecondExaminerScore.Value,
                exam.OrgUnitId,
                cancellationToken);

            score.ScoreFormula = await GetScoreFormulaAsync(exam.OrgUnitId, cancellationToken);
            score.ScoredAt = DateTime.UtcNow;

            // Check threshold
            var passingScore = exam.PassingScore ?? await GetCallPassingScoreAsync(exam.CallForCandidatesId, cancellationToken);
            if (passingScore.HasValue)
            {
                score.PassedThreshold = score.FinalScore >= passingScore.Value;

                // Auto-update candidacy status if below threshold
                if (!score.PassedThreshold.Value)
                {
                    await UpdateCandidacyStatusForFailedExamAsync(candidacy, cancellationToken);
                }
            }
        }

        if (isNew)
            await _scoreRepository.AddAsync(score, cancellationToken);
        else
            await _scoreRepository.UpdateAsync(score, cancellationToken);

        return ToScoreDto(score);
    }

    public async Task<ExamScoreDto> GetScoreAsync(int examId, int candidacyId, CancellationToken cancellationToken = default)
    {
        var scores = await _scoreRepository.FindAsync(
            s => s.ExamId == examId && s.CandidacyId == candidacyId,
            cancellationToken);

        var score = scores.FirstOrDefault()
            ?? throw new NotFoundException("ExamScore", $"ExamId={examId}, CandidacyId={candidacyId}");

        return ToScoreDto(score);
    }

    public async Task<IEnumerable<ExamScoreDto>> GetScoresByExamAsync(int examId, CancellationToken cancellationToken = default)
    {
        _ = await _examRepository.GetByIdAsync(examId, cancellationToken)
            ?? throw new NotFoundException(nameof(Exam), examId);

        var scores = await _scoreRepository.FindAsync(
            s => s.ExamId == examId, cancellationToken);

        return scores.Select(ToScoreDto);
    }

    // --- Appeals ---

    public async Task<ExamScoreDto> SubmitAppealAsync(SubmitAppealCommand command, CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.GetByIdAsync(command.ExamId, cancellationToken)
            ?? throw new NotFoundException(nameof(Exam), command.ExamId);

        // Validate appeal deadline exists
        if (!exam.AppealDeadline.HasValue)
            throw new ValidationException("AppealDeadline", "מבחן זה אינו מאפשר הגשת ערעורים");

        // Validate appeal is within deadline
        if (DateTime.UtcNow > exam.AppealDeadline.Value)
            throw new ValidationException("AppealDeadline", "תקופת הערעור הסתיימה");

        // Validate appeal score is within range
        if (command.AppealScore < 0 || command.AppealScore > exam.MaxScore)
            throw new ValidationException("AppealScore", $"ציון ערעור חייב להיות בין 0 ל-{exam.MaxScore}");

        // Find existing score record
        var existingScores = await _scoreRepository.FindAsync(
            s => s.ExamId == command.ExamId && s.CandidacyId == command.CandidacyId,
            cancellationToken);

        var score = existingScores.FirstOrDefault()
            ?? throw new NotFoundException("ExamScore", $"ExamId={command.ExamId}, CandidacyId={command.CandidacyId}");

        // Mark as appealed and set appeal score
        score.IsAppealed = true;
        score.AppealScore = command.AppealScore;

        // Recalculate final score using appeal score
        // The appeal score replaces the lower of the two examiner scores
        if (score.FirstExaminerScore.HasValue && score.SecondExaminerScore.HasValue)
        {
            var first = score.FirstExaminerScore.Value;
            var second = score.SecondExaminerScore.Value;
            var lowerIsFirst = first <= second;

            var newFirst = lowerIsFirst ? command.AppealScore : first;
            var newSecond = lowerIsFirst ? second : command.AppealScore;

            score.FinalScore = await CalculateFinalScore(
                newFirst, newSecond, exam.OrgUnitId, cancellationToken);

            score.ScoreFormula = await GetScoreFormulaAsync(exam.OrgUnitId, cancellationToken);

            // Re-check threshold
            var passingScore = exam.PassingScore ?? await GetCallPassingScoreAsync(exam.CallForCandidatesId, cancellationToken);
            if (passingScore.HasValue)
            {
                var previouslyPassed = score.PassedThreshold;
                score.PassedThreshold = score.FinalScore >= passingScore.Value;

                // If the candidate now passes the threshold after appeal, update candidacy status
                if (score.PassedThreshold.Value && previouslyPassed == false)
                {
                    var candidacy = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken);
                    if (candidacy != null)
                    {
                        await UpdateCandidacyStatusForPassedAppealAsync(candidacy, cancellationToken);
                    }
                }
            }
        }

        await _scoreRepository.UpdateAsync(score, cancellationToken);
        return ToScoreDto(score);
    }

    // --- Private helpers ---

    /// <summary>
    /// Calculates the final score from first and second examiner scores.
    /// Default formula: average. Can be overridden per org unit via business rules.
    /// </summary>
    internal async Task<decimal> CalculateFinalScore(
        decimal firstScore, decimal secondScore, int orgUnitId,
        CancellationToken cancellationToken = default)
    {
        var formula = await GetScoreFormulaAsync(orgUnitId, cancellationToken);

        return formula switch
        {
            "Max" => Math.Max(firstScore, secondScore),
            "Min" => Math.Min(firstScore, secondScore),
            "WeightedFirst" => firstScore * 0.6m + secondScore * 0.4m,
            "WeightedSecond" => firstScore * 0.4m + secondScore * 0.6m,
            _ => (firstScore + secondScore) / 2m // Default: Average
        };
    }

    /// <summary>
    /// Gets the score formula configured for the org unit via business rules.
    /// Returns "Average" if no specific formula is configured.
    /// </summary>
    internal async Task<string> GetScoreFormulaAsync(int orgUnitId, CancellationToken cancellationToken = default)
    {
        var rules = await _businessRuleRepository.FindAsync(
            r => r.OrgUnitId == orgUnitId
                 && r.RuleType == Domain.Enums.BusinessRuleType.ScoreCalculation
                 && r.IsActive,
            cancellationToken);

        var rule = rules.OrderBy(r => r.Priority).FirstOrDefault();
        return rule?.ActionParameters ?? "Average";
    }

    private async Task<decimal?> GetCallPassingScoreAsync(int callForCandidatesId, CancellationToken cancellationToken)
    {
        var call = await _callRepository.GetByIdAsync(callForCandidatesId, cancellationToken);
        return call?.MinScore;
    }

    /// <summary>
    /// When a candidate fails the exam threshold, find and apply the appropriate
    /// status transition (e.g., to "נכשל_במבחן") if one is configured.
    /// </summary>
    private async Task UpdateCandidacyStatusForFailedExamAsync(
        Candidacy candidacy, CancellationToken cancellationToken)
    {
        if (!candidacy.CurrentStatusId.HasValue || !candidacy.IsActive)
            return;

        // Look for a status with code containing "failed_exam" or "נכשל_במבחן"
        var failedStatuses = await _statusRepository.FindAsync(
            s => s.OrgUnitId == candidacy.OrgUnitId
                 && (s.Code == "failed_exam" || s.Code == "נכשל_במבחן"),
            cancellationToken);

        var failedStatus = failedStatuses.FirstOrDefault();
        if (failedStatus == null)
            return;

        // Verify the transition is allowed
        var transitions = await _transitionRepository.FindAsync(
            t => t.OrgUnitId == candidacy.OrgUnitId
                 && t.FromStatusId == candidacy.CurrentStatusId
                 && t.ToStatusId == failedStatus.Id,
            cancellationToken);

        if (!transitions.Any())
            return;

        // Record history
        var history = new CandidacyStatusHistory
        {
            CandidacyId = candidacy.Id,
            FromStatusId = candidacy.CurrentStatusId,
            ToStatusId = failedStatus.Id,
            Reason = "עדכון אוטומטי - ציון מבחן מתחת לסף",
            ChangedAt = DateTime.UtcNow
        };

        candidacy.CurrentStatusId = failedStatus.Id;
        if (failedStatus.IsFinal)
            candidacy.IsActive = false;

        await _candidacyRepository.UpdateAsync(candidacy, cancellationToken);
        await _historyRepository.AddAsync(history, cancellationToken);
    }

    private static ExamDto ToDto(Exam entity) =>
        new(entity.Id, entity.OrgUnitId, entity.CallForCandidatesId, entity.Name,
            entity.ExamDate, entity.Location, entity.MaxScore, entity.PassingScore,
            entity.FirstExaminerId, entity.SecondExaminerId, entity.AppealDeadline);

    /// <summary>
    /// When a candidate passes the threshold after a successful appeal,
    /// find and apply the appropriate status transition (e.g., to "עבר_מבחן") if configured.
    /// </summary>
    private async Task UpdateCandidacyStatusForPassedAppealAsync(
        Candidacy candidacy, CancellationToken cancellationToken)
    {
        if (!candidacy.CurrentStatusId.HasValue || !candidacy.IsActive)
            return;

        // Look for a status with code containing "passed_exam" or "עבר_מבחן"
        var passedStatuses = await _statusRepository.FindAsync(
            s => s.OrgUnitId == candidacy.OrgUnitId
                 && (s.Code == "passed_exam" || s.Code == "עבר_מבחן"),
            cancellationToken);

        var passedStatus = passedStatuses.FirstOrDefault();
        if (passedStatus == null)
            return;

        // Verify the transition is allowed
        var transitions = await _transitionRepository.FindAsync(
            t => t.OrgUnitId == candidacy.OrgUnitId
                 && t.FromStatusId == candidacy.CurrentStatusId
                 && t.ToStatusId == passedStatus.Id,
            cancellationToken);

        if (!transitions.Any())
            return;

        // Record history
        var history = new CandidacyStatusHistory
        {
            CandidacyId = candidacy.Id,
            FromStatusId = candidacy.CurrentStatusId,
            ToStatusId = passedStatus.Id,
            Reason = "עדכון אוטומטי - ערעור על ציון מבחן התקבל",
            ChangedAt = DateTime.UtcNow
        };

        candidacy.CurrentStatusId = passedStatus.Id;
        if (passedStatus.IsFinal)
            candidacy.IsActive = false;

        await _candidacyRepository.UpdateAsync(candidacy, cancellationToken);
        await _historyRepository.AddAsync(history, cancellationToken);
    }

    private static ExamScoreDto ToScoreDto(ExamScore entity) =>
        new(entity.Id, entity.ExamId, entity.CandidacyId,
            entity.FirstExaminerScore, entity.SecondExaminerScore,
            entity.FinalScore, entity.ScoreFormula, entity.PassedThreshold,
            entity.IsAppealed, entity.AppealScore, entity.ScoredAt);
}
