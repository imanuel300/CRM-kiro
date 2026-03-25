using System.Linq.Expressions;
using CandidacyManagement.Application.ThresholdChecks;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.ThresholdChecks;

public class ThresholdCheckServiceTests
{
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<ThresholdCondition>> _conditionRepoMock;
    private readonly Mock<IRepository<ThresholdCheckResult>> _resultRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<StatusTransition>> _transitionRepoMock;
    private readonly Mock<IRepository<CandidacyStatusHistory>> _historyRepoMock;
    private readonly Mock<IRepository<CandidacyCustomFieldValue>> _customFieldRepoMock;
    private readonly ThresholdCheckService _sut;

    public ThresholdCheckServiceTests()
    {
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _conditionRepoMock = new Mock<IRepository<ThresholdCondition>>();
        _resultRepoMock = new Mock<IRepository<ThresholdCheckResult>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        _historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();
        _customFieldRepoMock = new Mock<IRepository<CandidacyCustomFieldValue>>();

        _sut = new ThresholdCheckService(
            _candidacyRepoMock.Object,
            _contactRepoMock.Object,
            _conditionRepoMock.Object,
            _resultRepoMock.Object,
            _statusRepoMock.Object,
            _transitionRepoMock.Object,
            _historyRepoMock.Object,
            _customFieldRepoMock.Object);
    }

    #region CheckAllAsync

    [Fact]
    public async Task CheckAllAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var act = () => _sut.CheckAllAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CheckAllAsync_WithAutomaticAgeCondition_PassesWhenAgeAboveThreshold()
    {
        var candidacy = new Candidacy { Id = 1, ContactId = 10, CallForCandidatesId = 100, OrgUnitId = 1, IsActive = true, CurrentStatusId = 5 };
        var contact = new Contact { Id = 10, DateOfBirth = DateTime.UtcNow.AddYears(-30) };
        var condition = new ThresholdCondition
        {
            Id = 1, CallForCandidatesId = 100, FieldName = "Age", Operator = ">=",
            Value = "25", IsAutomatic = true, ConditionType = ConditionType.Age
        };

        SetupCandidacy(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _conditionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCondition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { condition });
        SetupEmptyResults();
        _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<ThresholdCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCheckResult e, CancellationToken _) => { e.Id = 1; return e; });

        var result = await _sut.CheckAllAsync(1);

        result.AllPassed.Should().BeTrue();
        result.Results.Should().HaveCount(1);
        result.Results.First().Passed.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAllAsync_WithAutomaticAgeCondition_FailsWhenAgeBelowThreshold()
    {
        var candidacy = new Candidacy { Id = 1, ContactId = 10, CallForCandidatesId = 100, OrgUnitId = 1, IsActive = true, CurrentStatusId = 5 };
        var contact = new Contact { Id = 10, DateOfBirth = DateTime.UtcNow.AddYears(-20) };
        var condition = new ThresholdCondition
        {
            Id = 1, CallForCandidatesId = 100, FieldName = "Age", Operator = ">=",
            Value = "25", IsAutomatic = true, ConditionType = ConditionType.Age
        };

        SetupCandidacy(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _conditionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCondition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { condition });
        SetupEmptyResults();
        SetupNoFailedStatus();
        _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<ThresholdCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCheckResult e, CancellationToken _) => { e.Id = 1; return e; });

        var result = await _sut.CheckAllAsync(1);

        result.AllPassed.Should().BeFalse();
        result.Results.First().Passed.Should().BeFalse();
    }

    [Fact]
    public async Task CheckAllAsync_NoConditions_ReturnsAllPassed()
    {
        var candidacy = new Candidacy { Id = 1, ContactId = 10, CallForCandidatesId = 100, OrgUnitId = 1, IsActive = true };

        SetupCandidacy(candidacy);
        _conditionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCondition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ThresholdCondition>());

        var result = await _sut.CheckAllAsync(1);

        result.AllPassed.Should().BeTrue();
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckAllAsync_FailedThreshold_UpdatesCandidacyStatus()
    {
        var candidacy = new Candidacy { Id = 1, ContactId = 10, CallForCandidatesId = 100, OrgUnitId = 1, IsActive = true, CurrentStatusId = 5 };
        var contact = new Contact { Id = 10, DateOfBirth = DateTime.UtcNow.AddYears(-20) };
        var condition = new ThresholdCondition
        {
            Id = 1, CallForCandidatesId = 100, FieldName = "Age", Operator = ">=",
            Value = "25", IsAutomatic = true, ConditionType = ConditionType.Age
        };
        var failedStatus = new StatusDefinition { Id = 99, OrgUnitId = 1, Code = "failed_threshold", IsFinal = true };
        var transition = new StatusTransition { Id = 1, OrgUnitId = 1, FromStatusId = 5, ToStatusId = 99 };

        SetupCandidacy(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _conditionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCondition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { condition });
        SetupEmptyResults();
        _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<ThresholdCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCheckResult e, CancellationToken _) => { e.Id = 1; return e; });
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { failedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { transition });

        await _sut.CheckAllAsync(1);

        _candidacyRepoMock.Verify(r => r.UpdateAsync(It.Is<Candidacy>(c => c.CurrentStatusId == 99), It.IsAny<CancellationToken>()), Times.Once);
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CheckSingleAsync

    [Fact]
    public async Task CheckSingleAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var act = () => _sut.CheckSingleAsync(999, 1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CheckSingleAsync_ConditionNotFound_ThrowsNotFoundException()
    {
        var candidacy = new Candidacy { Id = 1, CallForCandidatesId = 100 };
        SetupCandidacy(candidacy);
        _conditionRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCondition?)null);

        var act = () => _sut.CheckSingleAsync(1, 999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CheckSingleAsync_ConditionNotBelongingToCall_ThrowsBusinessRuleViolation()
    {
        var candidacy = new Candidacy { Id = 1, CallForCandidatesId = 100 };
        var condition = new ThresholdCondition { Id = 1, CallForCandidatesId = 200, IsAutomatic = true };

        SetupCandidacy(candidacy);
        _conditionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(condition);

        var act = () => _sut.CheckSingleAsync(1, 1);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task CheckSingleAsync_NonAutomaticCondition_ThrowsBusinessRuleViolation()
    {
        var candidacy = new Candidacy { Id = 1, CallForCandidatesId = 100 };
        var condition = new ThresholdCondition { Id = 1, CallForCandidatesId = 100, IsAutomatic = false };

        SetupCandidacy(candidacy);
        _conditionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(condition);

        var act = () => _sut.CheckSingleAsync(1, 1);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task CheckSingleAsync_ValidAgeCondition_ReturnsResult()
    {
        var candidacy = new Candidacy { Id = 1, ContactId = 10, CallForCandidatesId = 100, OrgUnitId = 1 };
        var contact = new Contact { Id = 10, DateOfBirth = DateTime.UtcNow.AddYears(-35) };
        var condition = new ThresholdCondition
        {
            Id = 1, CallForCandidatesId = 100, FieldName = "Age", Operator = ">=",
            Value = "30", IsAutomatic = true, ConditionType = ConditionType.Age
        };

        SetupCandidacy(candidacy);
        _contactRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _conditionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(condition);
        SetupEmptyResults();
        _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<ThresholdCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCheckResult e, CancellationToken _) => { e.Id = 1; return e; });

        var result = await _sut.CheckSingleAsync(1, 1);

        result.Passed.Should().BeTrue();
        result.IsAutomatic.Should().BeTrue();
    }

    #endregion

    #region ManualCheckAsync

    [Fact]
    public async Task ManualCheckAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var command = new ManualCheckCommand(999, 1, true, "הערות", 1);
        var act = () => _sut.ManualCheckAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ManualCheckAsync_ConditionNotBelongingToCall_ThrowsBusinessRuleViolation()
    {
        var candidacy = new Candidacy { Id = 1, CallForCandidatesId = 100 };
        var condition = new ThresholdCondition { Id = 1, CallForCandidatesId = 200 };

        SetupCandidacy(candidacy);
        _conditionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(condition);

        var command = new ManualCheckCommand(1, 1, true, null, 1);
        var act = () => _sut.ManualCheckAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task ManualCheckAsync_NewResult_CreatesAndReturnsDto()
    {
        var candidacy = new Candidacy { Id = 1, CallForCandidatesId = 100, OrgUnitId = 1, IsActive = true };
        var condition = new ThresholdCondition
        {
            Id = 1, CallForCandidatesId = 100, FieldName = "ניסיון",
            ConditionType = ConditionType.Custom, IsAutomatic = false
        };

        SetupCandidacy(candidacy);
        _conditionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(condition);
        _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCheckResult, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ThresholdCheckResult>());
        _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<ThresholdCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCheckResult e, CancellationToken _) => { e.Id = 1; return e; });

        var command = new ManualCheckCommand(1, 1, true, "עומד בדרישות", 5);
        var result = await _sut.ManualCheckAsync(command);

        result.Passed.Should().BeTrue();
        result.Notes.Should().Be("עומד בדרישות");
        result.IsAutomatic.Should().BeFalse();
        result.CheckedByUserId.Should().Be(5);
    }

    [Fact]
    public async Task ManualCheckAsync_ExistingResult_UpdatesAndReturnsDto()
    {
        var candidacy = new Candidacy { Id = 1, CallForCandidatesId = 100, OrgUnitId = 1, IsActive = true };
        var condition = new ThresholdCondition
        {
            Id = 1, CallForCandidatesId = 100, FieldName = "ניסיון",
            ConditionType = ConditionType.Custom, IsAutomatic = false
        };
        var existingResult = new ThresholdCheckResult
        {
            Id = 10, CandidacyId = 1, ThresholdConditionId = 1, Passed = false, Notes = "ישן"
        };

        SetupCandidacy(candidacy);
        _conditionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(condition);
        _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCheckResult, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingResult });

        var command = new ManualCheckCommand(1, 1, true, "עודכן", 5);
        var result = await _sut.ManualCheckAsync(command);

        result.Passed.Should().BeTrue();
        result.Notes.Should().Be("עודכן");
        _resultRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ThresholdCheckResult>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManualCheckAsync_FailedManualCheck_UpdatesCandidacyStatus()
    {
        var candidacy = new Candidacy { Id = 1, CallForCandidatesId = 100, OrgUnitId = 1, IsActive = true, CurrentStatusId = 5 };
        var condition = new ThresholdCondition { Id = 1, CallForCandidatesId = 100, FieldName = "ניסיון", ConditionType = ConditionType.Custom };
        var failedStatus = new StatusDefinition { Id = 99, OrgUnitId = 1, Code = "failed_threshold", IsFinal = true };
        var transition = new StatusTransition { Id = 1, OrgUnitId = 1, FromStatusId = 5, ToStatusId = 99 };

        SetupCandidacy(candidacy);
        _conditionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(condition);
        _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCheckResult, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ThresholdCheckResult>());
        _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<ThresholdCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCheckResult e, CancellationToken _) => { e.Id = 1; return e; });
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { failedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { transition });

        var command = new ManualCheckCommand(1, 1, false, "לא עומד בדרישות", 5);
        await _sut.ManualCheckAsync(command);

        _candidacyRepoMock.Verify(r => r.UpdateAsync(It.Is<Candidacy>(c => c.CurrentStatusId == 99), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetResultsAsync

    [Fact]
    public async Task GetResultsAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var act = () => _sut.GetResultsAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetResultsAsync_WithResults_ReturnsDtos()
    {
        var candidacy = new Candidacy { Id = 1, CallForCandidatesId = 100 };
        var condition = new ThresholdCondition { Id = 1, CallForCandidatesId = 100, FieldName = "Age", ConditionType = ConditionType.Age };
        var checkResult = new ThresholdCheckResult
        {
            Id = 10, CandidacyId = 1, ThresholdConditionId = 1, Passed = true,
            ActualValue = "30", IsAutomatic = true, CheckedAt = DateTime.UtcNow
        };

        SetupCandidacy(candidacy);
        _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCheckResult, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { checkResult });
        _conditionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(condition);

        var results = await _sut.GetResultsAsync(1);

        results.Should().HaveCount(1);
        results.First().Passed.Should().BeTrue();
        results.First().ActualValue.Should().Be("30");
    }

    #endregion

    #region Static Evaluation Methods

    [Theory]
    [InlineData(30, ">=", "25", true)]
    [InlineData(20, ">=", "25", false)]
    [InlineData(25, ">=", "25", true)]
    [InlineData(30, ">", "25", true)]
    [InlineData(25, ">", "25", false)]
    [InlineData(20, "<=", "25", true)]
    [InlineData(30, "<=", "25", false)]
    [InlineData(25, "==", "25", true)]
    [InlineData(26, "==", "25", false)]
    public void EvaluateAgeCondition_VariousOperators_ReturnsExpected(int age, string op, string requiredAge, bool expected)
    {
        var contact = new Contact { DateOfBirth = DateTime.UtcNow.AddYears(-age) };
        var condition = new ThresholdCondition { Operator = op, Value = requiredAge };

        var result = ThresholdCheckService.EvaluateAgeCondition(contact, condition, out _);

        result.Should().Be(expected);
    }

    [Fact]
    public void EvaluateAgeCondition_NullDateOfBirth_ReturnsFalse()
    {
        var contact = new Contact { DateOfBirth = null };
        var condition = new ThresholdCondition { Operator = ">=", Value = "25" };

        var result = ThresholdCheckService.EvaluateAgeCondition(contact, condition, out var actualValue);

        result.Should().BeFalse();
        actualValue.Should().BeNull();
    }

    [Fact]
    public void EvaluateAgeCondition_NullContact_ReturnsFalse()
    {
        var condition = new ThresholdCondition { Operator = ">=", Value = "25" };

        var result = ThresholdCheckService.EvaluateAgeCondition(null, condition, out _);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("80", ">=", "60", true)]
    [InlineData("50", ">=", "60", false)]
    [InlineData("60", ">=", "60", true)]
    public void EvaluateScoreCondition_VariousScores_ReturnsExpected(string score, string op, string required, bool expected)
    {
        var condition = new ThresholdCondition { Operator = op, Value = required };

        var result = ThresholdCheckService.EvaluateScoreCondition(condition, score, out _);

        result.Should().Be(expected);
    }

    [Fact]
    public void EvaluateScoreCondition_NullValue_ReturnsFalse()
    {
        var condition = new ThresholdCondition { Operator = ">=", Value = "60" };

        var result = ThresholdCheckService.EvaluateScoreCondition(condition, null, out _);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("תואר ראשון", "==", "תואר ראשון", true)]
    [InlineData("תואר שני", "==", "תואר ראשון", false)]
    [InlineData("תואר ראשון במשפטים", "contains", "משפטים", true)]
    [InlineData("תואר ראשון בהנדסה", "contains", "משפטים", false)]
    public void EvaluateEducationCondition_VariousValues_ReturnsExpected(string actual, string op, string required, bool expected)
    {
        var condition = new ThresholdCondition { Operator = op, Value = required };

        var result = ThresholdCheckService.EvaluateEducationCondition(condition, actual, out _);

        result.Should().Be(expected);
    }

    [Fact]
    public void EvaluateEducationCondition_NullValue_ReturnsFalse()
    {
        var condition = new ThresholdCondition { Operator = "==", Value = "תואר ראשון" };

        var result = ThresholdCheckService.EvaluateEducationCondition(condition, null, out _);

        result.Should().BeFalse();
    }

    #endregion

    #region Helpers

    private void SetupCandidacy(Candidacy candidacy)
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(candidacy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
    }

    private void SetupEmptyResults()
    {
        _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCheckResult, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ThresholdCheckResult>());
    }

    private void SetupNoFailedStatus()
    {
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<StatusDefinition>());
    }

    #endregion
}
