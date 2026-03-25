using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.CallsForCandidates;

public class CallForCandidatesService : ICallForCandidatesService
{
    private readonly IRepository<CallForCandidates> _callRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IRepository<ThresholdCondition> _thresholdRepository;
    private readonly IRepository<Position> _positionRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<StatusDefinition> _statusRepository;

    public CallForCandidatesService(
        IRepository<CallForCandidates> callRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IRepository<ThresholdCondition> thresholdRepository,
        IRepository<Position> positionRepository,
        IRepository<Candidacy> candidacyRepository,
        IRepository<StatusDefinition> statusRepository)
    {
        _callRepository = callRepository;
        _orgUnitRepository = orgUnitRepository;
        _thresholdRepository = thresholdRepository;
        _positionRepository = positionRepository;
        _candidacyRepository = candidacyRepository;
        _statusRepository = statusRepository;
    }

    public async Task<CallForCandidatesDto> CreateAsync(CreateCallForCandidatesCommand command, CancellationToken cancellationToken = default)
    {
        ValidateTitle(command.Title);
        ValidateDates(command.OpenDate, command.CloseDate);
        if (command.IsTender)
            ValidateTenderFields(command.MinScore);

        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        var entity = new CallForCandidates
        {
            OrgUnitId = command.OrgUnitId,
            Title = command.Title.Trim(),
            Description = command.Description,
            OpenDate = command.OpenDate,
            CloseDate = command.CloseDate,
            IsTender = command.IsTender,
            MinScore = command.IsTender ? command.MinScore : null,
            EligibilityConditions = command.IsTender ? command.EligibilityConditions : null,
            IsActive = true
        };

        await _callRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<CallForCandidatesDto> UpdateAsync(UpdateCallForCandidatesCommand command, CancellationToken cancellationToken = default)
    {
        ValidateTitle(command.Title);
        ValidateDates(command.OpenDate, command.CloseDate);

        if (command.IsTender)
            ValidateTenderFields(command.MinScore);

        var entity = await _callRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), command.Id);

        entity.Title = command.Title.Trim();
        entity.Description = command.Description;
        entity.OpenDate = command.OpenDate;
        entity.CloseDate = command.CloseDate;
        entity.IsTender = command.IsTender;
        entity.MinScore = command.IsTender ? command.MinScore : null;
        entity.EligibilityConditions = command.IsTender ? command.EligibilityConditions : null;

        await _callRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<CallForCandidatesDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _callRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), id);

        return ToDto(entity);
    }

    public async Task<CallForCandidatesDetailDto> GetDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _callRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), id);

        var thresholds = await _thresholdRepository.FindAsync(
            t => t.CallForCandidatesId == id, cancellationToken);

        var positions = await _positionRepository.FindAsync(
            p => p.CallForCandidatesId == id, cancellationToken);

        return new CallForCandidatesDetailDto(
            entity.Id, entity.OrgUnitId, entity.Title, entity.Description,
            entity.OpenDate, entity.CloseDate, entity.IsTender, entity.MinScore,
            entity.EligibilityConditions, entity.IsActive, entity.CreatedAt,
            thresholds.Select(ToThresholdDto),
            positions.Select(ToPositionDto));
    }

    public async Task<IEnumerable<CallForCandidatesDto>> ListAsync(CallForCandidatesQueryParams query, CancellationToken cancellationToken = default)
    {
        var results = await _callRepository.FindAsync(c =>
            (!query.OrgUnitId.HasValue || c.OrgUnitId == query.OrgUnitId.Value) &&
            (!query.IsActive.HasValue || c.IsActive == query.IsActive.Value) &&
            (!query.IsTender.HasValue || c.IsTender == query.IsTender.Value),
            cancellationToken);

        return results.Select(ToDto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _callRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), id);

        await _callRepository.DeleteAsync(entity, cancellationToken);
    }

    // --- Threshold Conditions ---

    public async Task<ThresholdConditionDto> AddThresholdConditionAsync(CreateThresholdConditionCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _callRepository.GetByIdAsync(command.CallForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), command.CallForCandidatesId);

        ValidateThresholdCondition(command);

        var entity = new ThresholdCondition
        {
            CallForCandidatesId = command.CallForCandidatesId,
            FieldName = command.FieldName.Trim(),
            Operator = command.Operator.Trim(),
            Value = command.Value.Trim(),
            IsAutomatic = command.IsAutomatic
        };

        await _thresholdRepository.AddAsync(entity, cancellationToken);
        return ToThresholdDto(entity);
    }

    public async Task RemoveThresholdConditionAsync(int conditionId, CancellationToken cancellationToken = default)
    {
        var entity = await _thresholdRepository.GetByIdAsync(conditionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ThresholdCondition), conditionId);

        await _thresholdRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<ThresholdConditionDto>> GetThresholdConditionsAsync(int callForCandidatesId, CancellationToken cancellationToken = default)
    {
        _ = await _callRepository.GetByIdAsync(callForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), callForCandidatesId);

        var conditions = await _thresholdRepository.FindAsync(
            t => t.CallForCandidatesId == callForCandidatesId, cancellationToken);

        return conditions.Select(ToThresholdDto);
    }

    // --- Positions ---

    public async Task<PositionDto> AddPositionAsync(CreatePositionCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _callRepository.GetByIdAsync(command.CallForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), command.CallForCandidatesId);

        ValidatePosition(command);

        var entity = new Position
        {
            CallForCandidatesId = command.CallForCandidatesId,
            Title = command.Title.Trim(),
            Description = command.Description,
            Vacancies = command.Vacancies
        };

        await _positionRepository.AddAsync(entity, cancellationToken);
        return ToPositionDto(entity);
    }

    public async Task RemovePositionAsync(int positionId, CancellationToken cancellationToken = default)
    {
        var entity = await _positionRepository.GetByIdAsync(positionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Position), positionId);

        await _positionRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<PositionDto>> GetPositionsAsync(int callForCandidatesId, CancellationToken cancellationToken = default)
    {
        _ = await _callRepository.GetByIdAsync(callForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), callForCandidatesId);

        var positions = await _positionRepository.FindAsync(
            p => p.CallForCandidatesId == callForCandidatesId, cancellationToken);

        return positions.Select(ToPositionDto);
    }

    // --- Closing Logic ---

    public async Task<bool> IsClosedAsync(int callForCandidatesId, CancellationToken cancellationToken = default)
    {
        var entity = await _callRepository.GetByIdAsync(callForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), callForCandidatesId);

        return entity.CloseDate.HasValue && entity.CloseDate.Value <= DateTime.UtcNow;
    }

    public async Task<ClosingSummaryDto> GetClosingSummaryAsync(int callForCandidatesId, CancellationToken cancellationToken = default)
    {
        var entity = await _callRepository.GetByIdAsync(callForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), callForCandidatesId);

        var candidacies = await _candidacyRepository.FindAsync(
            c => c.CallForCandidatesId == callForCandidatesId, cancellationToken);

        var candidacyList = candidacies.ToList();
        var totalCandidacies = candidacyList.Count;

        // Count rejected candidacies by checking if their current status is in the Rejected category
        var rejectedCount = 0;
        var metThresholdCount = 0;

        foreach (var candidacy in candidacyList)
        {
            if (candidacy.CurrentStatusId.HasValue)
            {
                var status = await _statusRepository.GetByIdAsync(candidacy.CurrentStatusId.Value, cancellationToken);
                if (status != null)
                {
                    if (status.Category == Domain.Enums.CandidacyStatusCategory.Rejected)
                        rejectedCount++;
                    else if (status.Category != Domain.Enums.CandidacyStatusCategory.Submitted)
                        metThresholdCount++;
                }
            }
        }

        return new ClosingSummaryDto(
            entity.Id,
            entity.Title,
            entity.CloseDate,
            totalCandidacies,
            metThresholdCount,
            rejectedCount);
    }

    // --- Private helpers ---

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Title", "כותרת היא שדה חובה");
    }

    private static void ValidateDates(DateTime openDate, DateTime? closeDate)
    {
        if (closeDate.HasValue && closeDate.Value <= openDate)
            throw new ValidationException("CloseDate", "תאריך סגירה חייב להיות אחרי תאריך פתיחה");
    }

    private static void ValidateTenderFields(decimal? minScore)
    {
        if (minScore.HasValue && minScore.Value < 0)
            throw new ValidationException("MinScore", "ציון סף לא יכול להיות שלילי");
    }

    private static void ValidateThresholdCondition(CreateThresholdConditionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.FieldName))
            throw new ValidationException("FieldName", "שם שדה הוא שדה חובה");
        if (string.IsNullOrWhiteSpace(command.Operator))
            throw new ValidationException("Operator", "אופרטור הוא שדה חובה");
        if (string.IsNullOrWhiteSpace(command.Value))
            throw new ValidationException("Value", "ערך הוא שדה חובה");
    }

    private static void ValidatePosition(CreatePositionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ValidationException("Title", "כותרת תפקיד היא שדה חובה");
        if (command.Vacancies < 1)
            throw new ValidationException("Vacancies", "מספר משרות חייב להיות לפחות 1");
    }

    private static CallForCandidatesDto ToDto(CallForCandidates entity) =>
        new(entity.Id, entity.OrgUnitId, entity.Title, entity.Description,
            entity.OpenDate, entity.CloseDate, entity.IsTender, entity.MinScore,
            entity.EligibilityConditions, entity.IsActive, entity.CreatedAt);

    private static ThresholdConditionDto ToThresholdDto(ThresholdCondition entity) =>
        new(entity.Id, entity.CallForCandidatesId, entity.FieldName,
            entity.Operator, entity.Value, entity.IsAutomatic);

    private static PositionDto ToPositionDto(Position entity) =>
        new(entity.Id, entity.CallForCandidatesId, entity.Title,
            entity.Description, entity.Vacancies);
}
