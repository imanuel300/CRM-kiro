using System.Linq.Expressions;
using CandidacyManagement.Application.Exams;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.Exams;

/// <summary>
/// Feature: unified-candidacy-management, Property 6: עקביות חישוב ציון סופי
/// (Final Score Calculation Consistency)
/// 
/// **Validates: Requirements 5.3**
/// 
/// For any exam with both first and second examiner scores entered, the final score
/// equals the result of applying the organizational unit's configured score formula
/// to those two scores. The final score is always deterministic and within valid bounds.
/// </summary>
public class ExamScoreCalculationPropertyTests
{
    /// <summary>
    /// Data container for a generated score calculation scenario.
    /// </summary>
    public record ScoreCalculationScenario(
        decimal FirstScore,
        decimal SecondScore,
        decimal MaxScore,
        int OrgUnitId,
        string Formula);

    /// <summary>
    /// Custom Arbitrary that generates valid score calculation scenarios:
    /// - MaxScore between 10 and 1000
    /// - FirstScore and SecondScore between 0 and MaxScore
    /// - Formula is one of the supported formulas
    /// </summary>
    private static Arbitrary<ScoreCalculationScenario> ScoreCalculationScenarioArb()
    {
        var formulas = Gen.Elements("Average", "Max", "Min", "WeightedFirst", "WeightedSecond");

        return Arb.From(
            from maxScoreInt in Gen.Choose(10, 1000)
            let maxScore = (decimal)maxScoreInt
            from firstInt in Gen.Choose(0, maxScoreInt)
            from secondInt in Gen.Choose(0, maxScoreInt)
            from orgUnitId in Gen.Choose(1, 100)
            from formula in formulas
            select new ScoreCalculationScenario(
                (decimal)firstInt,
                (decimal)secondInt,
                maxScore,
                orgUnitId,
                formula));
    }

    private static (ExamService service, Mock<IRepository<BusinessRule>> businessRuleRepo) SetupService()
    {
        var examRepoMock = new Mock<IRepository<Exam>>();
        var scoreRepoMock = new Mock<IRepository<ExamScore>>();
        var candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        var callRepoMock = new Mock<IRepository<CallForCandidates>>();
        var orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        var businessRuleRepoMock = new Mock<IRepository<BusinessRule>>();
        var statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        var transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        var historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();

        var service = new ExamService(
            examRepoMock.Object,
            scoreRepoMock.Object,
            candidacyRepoMock.Object,
            callRepoMock.Object,
            orgUnitRepoMock.Object,
            businessRuleRepoMock.Object,
            statusRepoMock.Object,
            transitionRepoMock.Object,
            historyRepoMock.Object);

        return (service, businessRuleRepoMock);
    }

    private static void SetupFormula(Mock<IRepository<BusinessRule>> businessRuleRepo, int orgUnitId, string formula)
    {
        if (formula == "Average")
        {
            // Average is the default when no rule is found
            businessRuleRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BusinessRule>());
        }
        else
        {
            businessRuleRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BusinessRule>
                {
                    new()
                    {
                        Id = 1,
                        OrgUnitId = orgUnitId,
                        RuleType = BusinessRuleType.ScoreCalculation,
                        ActionParameters = formula,
                        IsActive = true,
                        Priority = 1
                    }
                });
        }
    }

    /// <summary>
    /// Computes the expected final score for a given formula, mirroring the production logic.
    /// </summary>
    private static decimal ExpectedScore(decimal first, decimal second, string formula) =>
        formula switch
        {
            "Max" => Math.Max(first, second),
            "Min" => Math.Min(first, second),
            "WeightedFirst" => first * 0.6m + second * 0.4m,
            "WeightedSecond" => first * 0.4m + second * 0.6m,
            _ => (first + second) / 2m
        };

    /// <summary>
    /// Feature: unified-candidacy-management, Property 6: עקביות חישוב ציון סופי
    /// **Validates: Requirements 5.3**
    /// 
    /// For any valid first and second examiner scores and any configured formula,
    /// the final score always matches the expected formula result.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ExamScoreCalculationPropertyTests) })]
    public async Task<bool> FinalScoreMatchesConfiguredFormula(ScoreCalculationScenario scenario)
    {
        var (service, businessRuleRepo) = SetupService();
        SetupFormula(businessRuleRepo, scenario.OrgUnitId, scenario.Formula);

        var result = await service.CalculateFinalScore(
            scenario.FirstScore, scenario.SecondScore, scenario.OrgUnitId);

        var expected = ExpectedScore(scenario.FirstScore, scenario.SecondScore, scenario.Formula);

        return result == expected;
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 6: עקביות חישוב ציון סופי
    /// **Validates: Requirements 5.3**
    /// 
    /// For any valid scores and formula, the final score is always between 0 and MaxScore.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ExamScoreCalculationPropertyTests) })]
    public async Task<bool> FinalScoreIsWithinBounds(ScoreCalculationScenario scenario)
    {
        var (service, businessRuleRepo) = SetupService();
        SetupFormula(businessRuleRepo, scenario.OrgUnitId, scenario.Formula);

        var result = await service.CalculateFinalScore(
            scenario.FirstScore, scenario.SecondScore, scenario.OrgUnitId);

        return result >= 0m && result <= scenario.MaxScore;
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 6: עקביות חישוב ציון סופי
    /// **Validates: Requirements 5.3**
    /// 
    /// For any valid scores and formula, calling CalculateFinalScore twice with the
    /// same inputs always produces the same result (determinism).
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ExamScoreCalculationPropertyTests) })]
    public async Task<bool> FinalScoreIsDeterministic(ScoreCalculationScenario scenario)
    {
        var (service, businessRuleRepo) = SetupService();
        SetupFormula(businessRuleRepo, scenario.OrgUnitId, scenario.Formula);

        var result1 = await service.CalculateFinalScore(
            scenario.FirstScore, scenario.SecondScore, scenario.OrgUnitId);

        var result2 = await service.CalculateFinalScore(
            scenario.FirstScore, scenario.SecondScore, scenario.OrgUnitId);

        return result1 == result2;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<ScoreCalculationScenario> Arbitrary() => ScoreCalculationScenarioArb();
}
