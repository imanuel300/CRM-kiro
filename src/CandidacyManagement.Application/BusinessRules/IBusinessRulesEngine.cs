namespace CandidacyManagement.Application.BusinessRules;

/// <summary>
/// מנוע כללים עסקיים - הפעלת כללים מותנים ברמת יחידה ארגונית
/// </summary>
public interface IBusinessRulesEngine
{
    Task<RuleEvaluationResult> EvaluateAsync(BusinessRuleContext context, CancellationToken cancellationToken = default);
    Task<IEnumerable<BusinessRuleDto>> GetRulesForOrgUnitAsync(int orgUnitId, CancellationToken cancellationToken = default);
}
