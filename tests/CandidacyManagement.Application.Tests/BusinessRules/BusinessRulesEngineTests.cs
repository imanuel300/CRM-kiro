using System.Linq.Expressions;
using CandidacyManagement.Application.BusinessRules;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.BusinessRules;

public class BusinessRulesEngineTests
{
    private readonly Mock<IRepository<BusinessRule>> _ruleRepoMock;
    private readonly Mock<IRuleEvaluator> _duplicateEvaluatorMock;
    private readonly Mock<IRuleEvaluator> _thresholdEvaluatorMock;
    private readonly BusinessRulesEngine _sut;

    public BusinessRulesEngineTests()
    {
        _ruleRepoMock = new Mock<IRepository<BusinessRule>>();
        _duplicateEvaluatorMock = new Mock<IRuleEvaluator>();
        _thresholdEvaluatorMock = new Mock<IRuleEvaluator>();

        var evaluators = new List<KeyValuePair<BusinessRuleType, IRuleEvaluator>>
        {
            new(BusinessRuleType.DuplicatePrevention, _duplicateEvaluatorMock.Object),
            new(BusinessRuleType.ThresholdCheck, _thresholdEvaluatorMock.Object)
        };

        _sut = new BusinessRulesEngine(_ruleRepoMock.Object, evaluators);
    }

    #region EvaluateAsync

    [Fact]
    public async Task EvaluateAsync_WithNoRules_ReturnsEmptyResult()
    {
        // Arrange
        var context = new BusinessRuleContext(OrgUnitId: 10, RuleType: BusinessRuleType.DuplicatePrevention);
        SetupRules(Enumerable.Empty<BusinessRule>());

        // Act
        var result = await _sut.EvaluateAsync(context);

        // Assert
        result.Results.Should().BeEmpty();
        result.AllSatisfied.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WithSingleSatisfiedRule_ReturnsAllSatisfied()
    {
        // Arrange
        var rule = CreateRule(1, BusinessRuleType.DuplicatePrevention, priority: 1);
        var context = new BusinessRuleContext(OrgUnitId: 10, RuleType: BusinessRuleType.DuplicatePrevention);

        SetupRules(new[] { rule });
        _duplicateEvaluatorMock
            .Setup(e => e.EvaluateAsync(rule, context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleResult(1, "Duplicate Check", true, "OK"));

        // Act
        var result = await _sut.EvaluateAsync(context);

        // Assert
        result.AllSatisfied.Should().BeTrue();
        result.Results.Should().HaveCount(1);
    }

    [Fact]
    public async Task EvaluateAsync_WithFailedRule_ReturnsNotAllSatisfied()
    {
        // Arrange
        var rule = CreateRule(1, BusinessRuleType.DuplicatePrevention, priority: 1);
        var context = new BusinessRuleContext(OrgUnitId: 10, RuleType: BusinessRuleType.DuplicatePrevention);

        SetupRules(new[] { rule });
        _duplicateEvaluatorMock
            .Setup(e => e.EvaluateAsync(rule, context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleResult(1, "Duplicate Check", false, "כפילות נמצאה"));

        // Act
        var result = await _sut.EvaluateAsync(context);

        // Assert
        result.AllSatisfied.Should().BeFalse();
        result.FailedRules.Should().HaveCount(1);
    }

    [Fact]
    public async Task EvaluateAsync_EvaluatesRulesInPriorityOrder()
    {
        // Arrange
        var rule1 = CreateRule(1, BusinessRuleType.DuplicatePrevention, priority: 10);
        var rule2 = CreateRule(2, BusinessRuleType.DuplicatePrevention, priority: 1);
        var context = new BusinessRuleContext(OrgUnitId: 10, RuleType: BusinessRuleType.DuplicatePrevention);

        SetupRules(new[] { rule1, rule2 });

        var callOrder = new List<int>();
        _duplicateEvaluatorMock
            .Setup(e => e.EvaluateAsync(rule2, context, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add(2))
            .ReturnsAsync(new RuleResult(2, "Rule 2", true));
        _duplicateEvaluatorMock
            .Setup(e => e.EvaluateAsync(rule1, context, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add(1))
            .ReturnsAsync(new RuleResult(1, "Rule 1", true));

        // Act
        await _sut.EvaluateAsync(context);

        // Assert
        callOrder.Should().Equal(2, 1); // priority 1 first, then priority 10
    }

    [Fact]
    public async Task EvaluateAsync_StopsProcessingWhenRuleRequestsStop()
    {
        // Arrange
        var rule1 = CreateRule(1, BusinessRuleType.DuplicatePrevention, priority: 1);
        var rule2 = CreateRule(2, BusinessRuleType.DuplicatePrevention, priority: 2);
        var context = new BusinessRuleContext(OrgUnitId: 10, RuleType: BusinessRuleType.DuplicatePrevention);

        SetupRules(new[] { rule1, rule2 });
        _duplicateEvaluatorMock
            .Setup(e => e.EvaluateAsync(rule1, context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleResult(1, "Rule 1", false, "Stop", ShouldStopProcessing: true));

        // Act
        var result = await _sut.EvaluateAsync(context);

        // Assert
        result.Results.Should().HaveCount(1);
        _duplicateEvaluatorMock.Verify(
            e => e.EvaluateAsync(rule2, context, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_WithUnknownRuleType_ReturnsFailedResult()
    {
        // Arrange - use a rule type that has no evaluator registered
        var rule = CreateRule(1, BusinessRuleType.ScoreCalculation, priority: 1);
        var context = new BusinessRuleContext(OrgUnitId: 10, RuleType: BusinessRuleType.ScoreCalculation);

        SetupRules(new[] { rule });

        // Act
        var result = await _sut.EvaluateAsync(context);

        // Assert
        result.AllSatisfied.Should().BeFalse();
        result.Results.Should().HaveCount(1);
        result.Results[0].IsSatisfied.Should().BeFalse();
    }

    #endregion

    #region GetRulesForOrgUnitAsync

    [Fact]
    public async Task GetRulesForOrgUnitAsync_ReturnsAllRulesForOrgUnit()
    {
        // Arrange
        var rules = new[]
        {
            CreateRule(1, BusinessRuleType.DuplicatePrevention, priority: 2),
            CreateRule(2, BusinessRuleType.ThresholdCheck, priority: 1)
        };

        _ruleRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<BusinessRule, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = (await _sut.GetRulesForOrgUnitAsync(10)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Priority.Should().Be(1); // ordered by priority
        result[1].Priority.Should().Be(2);
    }

    [Fact]
    public async Task GetRulesForOrgUnitAsync_WhenNoRules_ReturnsEmpty()
    {
        // Arrange
        _ruleRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<BusinessRule, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BusinessRule>());

        // Act
        var result = await _sut.GetRulesForOrgUnitAsync(10);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Helpers

    private static BusinessRule CreateRule(int id, BusinessRuleType type, int priority, bool isActive = true)
    {
        return new BusinessRule
        {
            Id = id,
            OrgUnitId = 10,
            RuleType = type,
            Name = $"Rule {id}",
            IsActive = isActive,
            Priority = priority
        };
    }

    private void SetupRules(IEnumerable<BusinessRule> rules)
    {
        _ruleRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<BusinessRule, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);
    }

    #endregion
}
