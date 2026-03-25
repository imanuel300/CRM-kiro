using CandidacyManagement.Domain.Entities;

namespace CandidacyManagement.Application.BusinessRules;

/// <summary>
/// ממשק להערכת כלל עסקי בודד
/// </summary>
public interface IRuleEvaluator
{
    Task<RuleResult> EvaluateAsync(BusinessRule rule, BusinessRuleContext context, CancellationToken cancellationToken = default);
}
