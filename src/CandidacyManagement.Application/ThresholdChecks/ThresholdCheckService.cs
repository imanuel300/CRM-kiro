using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.ThresholdChecks;

/// <summary>
/// מנוע בדיקת תנאי סף - בדיקה אוטומטית וידנית של עמידה בתנאי סף
/// </summary>
public class ThresholdCheckService : IThresholdCheckService
{
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<ThresholdCondition> _conditionRepository;
    private readonly IRepository<ThresholdCheckResult> _resultRepository;
    private readonly IRepository<StatusDefinition> _statusRepository;
    private readonly IRepository<StatusTransition> _transitionRepository;
    private readonly IRepository<CandidacyStatusHistory> _historyRepository;
    private readonly IRepository<CandidacyCustomFieldValue> _customFieldValueRepository;

    public ThresholdCheckService(
        IRepository<Candidacy> candidacyRepository,
        IRepository<Contact> contactRepository,
        IRepository<ThresholdCondition> conditionRepository,
        IRepository<ThresholdCheckResult> resultRepository,
        IRepository<StatusDefinition> statusRepository,
        IRepository<StatusTransition> transitionRepository,
        IRepository<CandidacyStatusHistory> historyRepository,
        IRepository<CandidacyCustomFieldValue> customFieldValueRepository)
    {
        _candidacyRepository = candidacyRepository;
        _contactRepository = contactRepository;
        _conditionRepository = conditionRepository;
        _resultRepository = resultRepository;
        _statusRepository = statusRepository;
        _transitionRepository = transitionRepository;
        _historyRepository = historyRepository;
        _customFieldValueRepository = customFieldValueRepository;
    }

    public async Task<CheckAllResultDto> CheckAllAsync(int candidacyId, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        var conditions = await _conditionRepository.FindAsync(
            c => c.CallForCandidatesId == candidacy.CallForCandidatesId, cancellationToken);

        var results = new List<ThresholdCheckResultDto>();

        foreach (var condition in conditions)
        {
            if (condition.IsAutomatic)
            {
                var result = await EvaluateAutomaticConditionAsync(candidacy, condition, cancellationToken);
                results.Add(result);
            }
            // תנאים לא אוטומטיים - בודקים אם כבר יש תוצאה קיימת
            else
            {
                var existing = await GetExistingResultAsync(candidacyId, condition.Id, cancellationToken);
                if (existing != null)
                    results.Add(existing);
            }
        }

        var allPassed = results.All(r => r.Passed);

        // עדכון סטטוס מועמדות אם לא עמד בתנאי סף
        if (!allPassed)
        {
            await UpdateCandidacyStatusForFailedThresholdAsync(candidacy, cancellationToken);
        }

        return new CheckAllResultDto(candidacyId, allPassed, results);
    }

    public async Task<ThresholdCheckResultDto> CheckSingleAsync(int candidacyId, int conditionId, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        var condition = await _conditionRepository.GetByIdAsync(conditionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ThresholdCondition), conditionId);

        if (condition.CallForCandidatesId != candidacy.CallForCandidatesId)
            throw new BusinessRuleViolationException("תנאי הסף אינו שייך לקול הקורא של מועמדות זו");

        if (!condition.IsAutomatic)
            throw new BusinessRuleViolationException("תנאי זה אינו אוטומטי - יש להשתמש בבדיקה ידנית");

        return await EvaluateAutomaticConditionAsync(candidacy, condition, cancellationToken);
    }

    public async Task<ThresholdCheckResultDto> ManualCheckAsync(ManualCheckCommand command, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        var condition = await _conditionRepository.GetByIdAsync(command.ConditionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ThresholdCondition), command.ConditionId);

        if (condition.CallForCandidatesId != candidacy.CallForCandidatesId)
            throw new BusinessRuleViolationException("תנאי הסף אינו שייך לקול הקורא של מועמדות זו");

        // חיפוש תוצאה קיימת לעדכון
        var existingResults = await _resultRepository.FindAsync(
            r => r.CandidacyId == command.CandidacyId && r.ThresholdConditionId == command.ConditionId,
            cancellationToken);

        var existing = existingResults.FirstOrDefault();

        if (existing != null)
        {
            existing.Passed = command.Passed;
            existing.Notes = command.Notes;
            existing.CheckedByUserId = command.UserId;
            existing.CheckedAt = DateTime.UtcNow;
            existing.IsAutomatic = false;
            await _resultRepository.UpdateAsync(existing, cancellationToken);
            return ToDto(existing, condition);
        }

        var entity = new ThresholdCheckResult
        {
            CandidacyId = command.CandidacyId,
            ThresholdConditionId = command.ConditionId,
            Passed = command.Passed,
            Notes = command.Notes,
            IsAutomatic = false,
            CheckedByUserId = command.UserId,
            CheckedAt = DateTime.UtcNow
        };

        await _resultRepository.AddAsync(entity, cancellationToken);

        // אם לא עבר - עדכון סטטוס מועמדות
        if (!command.Passed)
        {
            await UpdateCandidacyStatusForFailedThresholdAsync(candidacy, cancellationToken);
        }

        return ToDto(entity, condition);
    }

    public async Task<IEnumerable<ThresholdCheckResultDto>> GetResultsAsync(int candidacyId, CancellationToken cancellationToken = default)
    {
        _ = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        var results = await _resultRepository.FindAsync(
            r => r.CandidacyId == candidacyId, cancellationToken);

        var dtos = new List<ThresholdCheckResultDto>();
        foreach (var result in results)
        {
            var condition = await _conditionRepository.GetByIdAsync(result.ThresholdConditionId, cancellationToken);
            if (condition != null)
                dtos.Add(ToDto(result, condition));
        }

        return dtos;
    }

    // --- Private helpers ---

    /// <summary>
    /// הערכה אוטומטית של תנאי סף בודד
    /// </summary>
    internal async Task<ThresholdCheckResultDto> EvaluateAutomaticConditionAsync(
        Candidacy candidacy, ThresholdCondition condition, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(candidacy.ContactId, cancellationToken);
        string? actualValue = null;
        bool passed;

        switch (condition.ConditionType)
        {
            case ConditionType.Age:
                passed = EvaluateAgeCondition(contact, condition, out actualValue);
                break;

            case ConditionType.Score:
                passed = EvaluateScoreCondition(condition, await GetCustomFieldValueAsync(
                    candidacy.Id, condition.FieldName, cancellationToken), out actualValue);
                break;

            case ConditionType.Education:
                passed = EvaluateEducationCondition(condition, await GetCustomFieldValueAsync(
                    candidacy.Id, condition.FieldName, cancellationToken), out actualValue);
                break;

            default:
                // Custom - לא ניתן לבדיקה אוטומטית
                passed = false;
                actualValue = "לא ניתן לבדיקה אוטומטית";
                break;
        }

        // שמירת/עדכון תוצאה
        var existingResults = await _resultRepository.FindAsync(
            r => r.CandidacyId == candidacy.Id && r.ThresholdConditionId == condition.Id,
            cancellationToken);

        var existing = existingResults.FirstOrDefault();

        if (existing != null)
        {
            existing.Passed = passed;
            existing.ActualValue = actualValue;
            existing.IsAutomatic = true;
            existing.CheckedAt = DateTime.UtcNow;
            await _resultRepository.UpdateAsync(existing, cancellationToken);
            return ToDto(existing, condition);
        }

        var entity = new ThresholdCheckResult
        {
            CandidacyId = candidacy.Id,
            ThresholdConditionId = condition.Id,
            Passed = passed,
            ActualValue = actualValue,
            IsAutomatic = true,
            CheckedAt = DateTime.UtcNow
        };

        await _resultRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity, condition);
    }

    internal static bool EvaluateAgeCondition(Contact? contact, ThresholdCondition condition, out string? actualValue)
    {
        if (contact?.DateOfBirth == null)
        {
            actualValue = null;
            return false;
        }

        var today = DateTime.UtcNow.Date;
        var age = today.Year - contact.DateOfBirth.Value.Year;
        if (contact.DateOfBirth.Value.Date > today.AddYears(-age))
            age--;

        actualValue = age.ToString();

        if (!int.TryParse(condition.Value, out var requiredValue))
            return false;

        return EvaluateComparison(age, requiredValue, condition.Operator);
    }

    internal static bool EvaluateScoreCondition(ThresholdCondition condition, string? fieldValue, out string? actualValue)
    {
        actualValue = fieldValue;

        if (string.IsNullOrEmpty(fieldValue) || !decimal.TryParse(fieldValue, out var score))
            return false;

        if (!decimal.TryParse(condition.Value, out var requiredScore))
            return false;

        return EvaluateComparison(score, requiredScore, condition.Operator);
    }

    internal static bool EvaluateEducationCondition(ThresholdCondition condition, string? fieldValue, out string? actualValue)
    {
        actualValue = fieldValue;

        if (string.IsNullOrEmpty(fieldValue))
            return false;

        // השוואת מחרוזות - שוויון או הכלה
        return condition.Operator switch
        {
            "==" or "=" => string.Equals(fieldValue.Trim(), condition.Value.Trim(), StringComparison.OrdinalIgnoreCase),
            "contains" => fieldValue.Trim().Contains(condition.Value.Trim(), StringComparison.OrdinalIgnoreCase),
            _ => string.Equals(fieldValue.Trim(), condition.Value.Trim(), StringComparison.OrdinalIgnoreCase)
        };
    }

    internal static bool EvaluateComparison<T>(T actual, T required, string op) where T : IComparable<T>
    {
        return op switch
        {
            ">=" or "גדול_שווה" => actual.CompareTo(required) >= 0,
            ">" or "גדול" => actual.CompareTo(required) > 0,
            "<=" or "קטן_שווה" => actual.CompareTo(required) <= 0,
            "<" or "קטן" => actual.CompareTo(required) < 0,
            "==" or "=" or "שווה" => actual.CompareTo(required) == 0,
            "!=" or "שונה" => actual.CompareTo(required) != 0,
            _ => actual.CompareTo(required) >= 0 // ברירת מחדל: גדול או שווה
        };
    }

    private async Task<string?> GetCustomFieldValueAsync(int candidacyId, string fieldName, CancellationToken cancellationToken)
    {
        var values = await _customFieldValueRepository.FindAsync(
            v => v.CandidacyId == candidacyId, cancellationToken);

        // חיפוש לפי שם שדה - נדרש join עם CustomFieldDefinition
        // כאן נחזיר את הערך הראשון שנמצא (פישוט)
        return values.FirstOrDefault()?.Value;
    }

    private async Task<ThresholdCheckResultDto?> GetExistingResultAsync(
        int candidacyId, int conditionId, CancellationToken cancellationToken)
    {
        var results = await _resultRepository.FindAsync(
            r => r.CandidacyId == candidacyId && r.ThresholdConditionId == conditionId,
            cancellationToken);

        var result = results.FirstOrDefault();
        if (result == null) return null;

        var condition = await _conditionRepository.GetByIdAsync(conditionId, cancellationToken);
        return condition != null ? ToDto(result, condition) : null;
    }

    /// <summary>
    /// עדכון סטטוס מועמדות כאשר לא עמד בתנאי סף
    /// </summary>
    private async Task UpdateCandidacyStatusForFailedThresholdAsync(
        Candidacy candidacy, CancellationToken cancellationToken)
    {
        if (!candidacy.CurrentStatusId.HasValue || !candidacy.IsActive)
            return;

        // חיפוש סטטוס "נגרע תנאי סף"
        var failedStatuses = await _statusRepository.FindAsync(
            s => s.OrgUnitId == candidacy.OrgUnitId
                 && (s.Code == "failed_threshold" || s.Code == "נגרע_תנאי_סף"),
            cancellationToken);

        var failedStatus = failedStatuses.FirstOrDefault();
        if (failedStatus == null)
            return;

        // וידוא שהמעבר מותר
        var transitions = await _transitionRepository.FindAsync(
            t => t.OrgUnitId == candidacy.OrgUnitId
                 && t.FromStatusId == candidacy.CurrentStatusId
                 && t.ToStatusId == failedStatus.Id,
            cancellationToken);

        if (!transitions.Any())
            return;

        var history = new CandidacyStatusHistory
        {
            CandidacyId = candidacy.Id,
            FromStatusId = candidacy.CurrentStatusId,
            ToStatusId = failedStatus.Id,
            Reason = "עדכון אוטומטי - לא עמד בתנאי סף",
            ChangedAt = DateTime.UtcNow
        };

        candidacy.CurrentStatusId = failedStatus.Id;
        if (failedStatus.IsFinal)
            candidacy.IsActive = false;

        await _candidacyRepository.UpdateAsync(candidacy, cancellationToken);
        await _historyRepository.AddAsync(history, cancellationToken);
    }

    private static ThresholdCheckResultDto ToDto(ThresholdCheckResult entity, ThresholdCondition condition) =>
        new(entity.Id, entity.CandidacyId, entity.ThresholdConditionId,
            condition.FieldName, condition.ConditionType,
            entity.Passed, entity.ActualValue, entity.Notes,
            entity.IsAutomatic, entity.CheckedByUserId, entity.CheckedAt);
}
