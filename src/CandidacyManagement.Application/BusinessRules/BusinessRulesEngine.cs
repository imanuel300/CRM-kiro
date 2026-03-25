using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.BusinessRules;

/// <summary>
/// מימוש מנוע כללים עסקיים - טוען כללים פעילים ומפעיל מעריכים בסדר עדיפות
/// </summary>
public class BusinessRulesEngine : IBusinessRulesEngine
{
    private readonly IRepository<BusinessRule> _ruleRepo;
    private readonly Dictionary<BusinessRuleType, IRuleEvaluator> _evaluators;

    public BusinessRulesEngine(
        IRepository<BusinessRule> ruleRepo,
        IEnumerable<KeyValuePair<BusinessRuleType, IRuleEvaluator>> evaluators)
    {
        _ruleRepo = ruleRepo;
        _evaluators = new Dictionary<BusinessRuleType, IRuleEvaluator>(evaluators);
    }

    public async Task<RuleEvaluationResult> EvaluateAsync(BusinessRuleContext context, CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepo.FindAsync(
            r => r.OrgUnitId == context.OrgUnitId && r.RuleType == context.RuleType && r.IsActive,
            cancellationToken);

        var orderedRules = rules.OrderBy(r => r.Priority).ToList();
        var results = new List<RuleResult>();

        foreach (var rule in orderedRules)
        {
            if (!_evaluators.TryGetValue(rule.RuleType, out var evaluator))
            {
                results.Add(new RuleResult(
                    RuleId: rule.Id,
                    RuleName: rule.Name,
                    IsSatisfied: false,
                    Message: $"לא נמצא מעריך עבור סוג כלל {rule.RuleType}"));
                continue;
            }

            var result = await evaluator.EvaluateAsync(rule, context, cancellationToken);
            results.Add(result);

            if (result.ShouldStopProcessing)
                break;
        }

        return new RuleEvaluationResult(results);
    }

    public async Task<IEnumerable<BusinessRuleDto>> GetRulesForOrgUnitAsync(int orgUnitId, CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepo.FindAsync(
            r => r.OrgUnitId == orgUnitId,
            cancellationToken);

        return rules.OrderBy(r => r.Priority).Select(r => new BusinessRuleDto(
            r.Id, r.OrgUnitId, r.RuleType, r.Name,
            r.ConditionExpression, r.ActionType, r.ActionParameters,
            r.IsActive, r.Priority));
    }
}
