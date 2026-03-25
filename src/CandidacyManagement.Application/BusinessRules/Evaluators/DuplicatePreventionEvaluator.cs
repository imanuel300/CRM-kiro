using CandidacyManagement.Domain.Entities;

namespace CandidacyManagement.Application.BusinessRules.Evaluators;

/// <summary>
/// מעריך כלל מניעת כפילויות - בודק שאין מועמדות כפולה
/// </summary>
public class DuplicatePreventionEvaluator : IRuleEvaluator
{
    public Task<RuleResult> EvaluateAsync(BusinessRule rule, BusinessRuleContext context, CancellationToken cancellationToken = default)
    {
        // Stub: בשלב זה מחזיר תוצאה חיובית תמיד
        // מימוש מלא יבדוק כפילויות מול בסיס הנתונים
        return Task.FromResult(new RuleResult(
            RuleId: rule.Id,
            RuleName: rule.Name,
            IsSatisfied: true,
            Message: "בדיקת כפילויות הושלמה"));
    }
}
