using CandidacyManagement.Domain.Entities;

namespace CandidacyManagement.Application.BusinessRules.Evaluators;

/// <summary>
/// מעריך כלל בדיקת ציון סף - בודק עמידה בתנאי סף מספרי
/// </summary>
public class ThresholdCheckEvaluator : IRuleEvaluator
{
    public Task<RuleResult> EvaluateAsync(BusinessRule rule, BusinessRuleContext context, CancellationToken cancellationToken = default)
    {
        // Stub: בשלב זה מחזיר תוצאה חיובית תמיד
        // מימוש מלא יבדוק ציון סף מול ערכי המועמדות
        return Task.FromResult(new RuleResult(
            RuleId: rule.Id,
            RuleName: rule.Name,
            IsSatisfied: true,
            Message: "בדיקת ציון סף הושלמה"));
    }
}
