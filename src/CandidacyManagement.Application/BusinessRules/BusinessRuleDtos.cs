using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.BusinessRules;

/// <summary>
/// DTO לכלל עסקי
/// </summary>
public record BusinessRuleDto(
    int Id,
    int OrgUnitId,
    BusinessRuleType RuleType,
    string Name,
    string? ConditionExpression,
    string? ActionType,
    string? ActionParameters,
    bool IsActive,
    int Priority);

/// <summary>
/// הקשר להפעלת כלל עסקי
/// </summary>
public record BusinessRuleContext(
    int OrgUnitId,
    BusinessRuleType RuleType,
    int? CandidacyId = null,
    int? ContactId = null,
    int? CallForCandidatesId = null,
    Dictionary<string, object>? Parameters = null);

/// <summary>
/// תוצאת הערכת כלל בודד
/// </summary>
public record RuleResult(
    int RuleId,
    string RuleName,
    bool IsSatisfied,
    string? Message = null,
    bool ShouldStopProcessing = false);

/// <summary>
/// תוצאת הערכת כל הכללים
/// </summary>
public record RuleEvaluationResult(IReadOnlyList<RuleResult> Results)
{
    public bool AllSatisfied => Results.All(r => r.IsSatisfied);
    public IEnumerable<RuleResult> FailedRules => Results.Where(r => !r.IsSatisfied);
}
