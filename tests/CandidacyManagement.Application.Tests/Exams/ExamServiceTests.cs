using System.Linq.Expressions;
using CandidacyManagement.Application.Exams;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Exams;

public class ExamServiceTests
{
    private readonly Mock<IRepository<Exam>> _examRepoMock;
    private readonly Mock<IRepository<ExamScore>> _scoreRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<CallForCandidates>> _callRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<BusinessRule>> _businessRuleRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<StatusTransition>> _transitionRepoMock;
    private readonly Mock<IRepository<CandidacyStatusHistory>> _historyRepoMock;
    private readonly ExamService _sut;

    public ExamServiceTests()
    {
        _examRepoMock = new Mock<IRepository<Exam>>();
        _scoreRepoMock = new Mock<IRepository<ExamScore>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _callRepoMock = new Mock<IRepository<CallForCandidates>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _businessRuleRepoMock = new Mock<IRepository<BusinessRule>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        _historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();

        _sut = new ExamService(
            _examRepoMock.Object,
            _scoreRepoMock.Object,
            _candidacyRepoMock.Object,
            _callRepoMock.Object,
            _orgUnitRepoMock.Object,
            _businessRuleRepoMock.Object,
            _statusRepoMock.Object,
            _transitionRepoMock.Object,
            _historyRepoMock.Object);
    }

    #region Create Exam

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsExamDto()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });
        _examRepoMock.Setup(r => r.AddAsync(It.IsAny<Exam>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam e, CancellationToken _) => { e.Id = 100; return e; });

        var command = new CreateExamCommand(1, 10, "מבחן כתיבה", DateTime.UtcNow.AddDays(30),
            "ירושלים", 100m, 60m, 5, 6, DateTime.UtcNow.AddDays(45));

        var result = await _sut.CreateAsync(command);

        result.Should().NotBeNull();
        result.Name.Should().Be("מבחן כתיבה");
        result.MaxScore.Should().Be(100m);
        result.PassingScore.Should().Be(60m);
        result.FirstExaminerId.Should().Be(5);
        result.SecondExaminerId.Should().Be(6);
    }

    [Fact]
    public async Task CreateAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var command = new CreateExamCommand(999, 10, "מבחן", DateTime.UtcNow, null, 100m, null, null, null, null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });

        var command = new CreateExamCommand(1, 10, "", DateTime.UtcNow, null, 100m, null, null, null, null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ZeroMaxScore_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });

        var command = new CreateExamCommand(1, 10, "מבחן", DateTime.UtcNow, null, 0m, null, null, null, null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Update Exam

    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsUpdatedDto()
    {
        var exam = new Exam { Id = 1, OrgUnitId = 1, Name = "Old Name", MaxScore = 100m };
        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        var command = new UpdateExamCommand(1, "New Name", DateTime.UtcNow, "תל אביב", 200m, 80m, 7, 8, null);

        var result = await _sut.UpdateAsync(command);

        result.Name.Should().Be("New Name");
        result.MaxScore.Should().Be(200m);
        result.Location.Should().Be("תל אביב");
    }

    [Fact]
    public async Task UpdateAsync_ExamNotFound_ThrowsNotFoundException()
    {
        _examRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);

        var command = new UpdateExamCommand(999, "Name", DateTime.UtcNow, null, 100m, null, null, null, null);

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Delete Exam

    [Fact]
    public async Task DeleteAsync_ExamNotFound_ThrowsNotFoundException()
    {
        _examRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Submit Score

    [Fact]
    public async Task SubmitScoreAsync_BothScores_CalculatesFinalScoreAverage()
    {
        var exam = new Exam { Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, IsActive = true };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ExamScore>());
        _scoreRepoMock.Setup(r => r.AddAsync(It.IsAny<ExamScore>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExamScore e, CancellationToken _) => { e.Id = 50; return e; });
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BusinessRule>());
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });

        var command = new SubmitScoreCommand(1, 5, 80m, 90m);

        var result = await _sut.SubmitScoreAsync(command);

        result.FinalScore.Should().Be(85m); // (80 + 90) / 2
        result.FirstExaminerScore.Should().Be(80m);
        result.SecondExaminerScore.Should().Be(90m);
        result.ScoredAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitScoreAsync_OnlyFirstExaminer_NoFinalScore()
    {
        var exam = new Exam { Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, IsActive = true };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ExamScore>());
        _scoreRepoMock.Setup(r => r.AddAsync(It.IsAny<ExamScore>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExamScore e, CancellationToken _) => { e.Id = 50; return e; });

        var command = new SubmitScoreCommand(1, 5, 80m, null);

        var result = await _sut.SubmitScoreAsync(command);

        result.FirstExaminerScore.Should().Be(80m);
        result.SecondExaminerScore.Should().BeNull();
        result.FinalScore.Should().BeNull();
    }

    [Fact]
    public async Task SubmitScoreAsync_ScoreExceedsMax_ThrowsValidationException()
    {
        var exam = new Exam { Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, IsActive = true };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        var command = new SubmitScoreCommand(1, 5, 150m, null);

        var act = () => _sut.SubmitScoreAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitScoreAsync_NegativeScore_ThrowsValidationException()
    {
        var exam = new Exam { Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, IsActive = true };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        var command = new SubmitScoreCommand(1, 5, -5m, null);

        var act = () => _sut.SubmitScoreAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitScoreAsync_BelowThreshold_UpdatesCandidacyStatus()
    {
        var exam = new Exam { Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m, PassingScore = 60m };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };
        var failedStatus = new StatusDefinition { Id = 20, OrgUnitId = 1, Code = "failed_exam", IsFinal = true };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ExamScore>());
        _scoreRepoMock.Setup(r => r.AddAsync(It.IsAny<ExamScore>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExamScore e, CancellationToken _) => { e.Id = 50; return e; });
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BusinessRule>());
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition> { failedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusTransition> { new() { Id = 1, FromStatusId = 10, ToStatusId = 20, OrgUnitId = 1 } });

        var command = new SubmitScoreCommand(1, 5, 30m, 40m);

        var result = await _sut.SubmitScoreAsync(command);

        result.FinalScore.Should().Be(35m); // (30 + 40) / 2
        result.PassedThreshold.Should().BeFalse();
        candidacy.CurrentStatusId.Should().Be(20);
        candidacy.IsActive.Should().BeFalse();
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitScoreAsync_AboveThreshold_PassedThresholdTrue()
    {
        var exam = new Exam { Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m, PassingScore = 60m };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ExamScore>());
        _scoreRepoMock.Setup(r => r.AddAsync(It.IsAny<ExamScore>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExamScore e, CancellationToken _) => { e.Id = 50; return e; });
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BusinessRule>());
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });

        var command = new SubmitScoreCommand(1, 5, 80m, 90m);

        var result = await _sut.SubmitScoreAsync(command);

        result.FinalScore.Should().Be(85m);
        result.PassedThreshold.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitScoreAsync_UpdatesExistingScore()
    {
        var exam = new Exam { Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, IsActive = true };
        var existingScore = new ExamScore { Id = 50, ExamId = 1, CandidacyId = 5, FirstExaminerScore = 70m };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExamScore> { existingScore });
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BusinessRule>());
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });

        var command = new SubmitScoreCommand(1, 5, null, 80m);

        var result = await _sut.SubmitScoreAsync(command);

        result.FirstExaminerScore.Should().Be(70m);
        result.SecondExaminerScore.Should().Be(80m);
        result.FinalScore.Should().Be(75m); // (70 + 80) / 2
        _scoreRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ExamScore>(), It.IsAny<CancellationToken>()), Times.Once);
        _scoreRepoMock.Verify(r => r.AddAsync(It.IsAny<ExamScore>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Score Calculation Formulas

    [Fact]
    public async Task CalculateFinalScore_DefaultAverage()
    {
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BusinessRule>());

        var result = await _sut.CalculateFinalScore(80m, 90m, 1);

        result.Should().Be(85m);
    }

    [Fact]
    public async Task CalculateFinalScore_MaxFormula()
    {
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessRule>
            {
                new() { Id = 1, OrgUnitId = 1, RuleType = Domain.Enums.BusinessRuleType.ScoreCalculation, ActionParameters = "Max", IsActive = true, Priority = 1 }
            });

        var result = await _sut.CalculateFinalScore(80m, 90m, 1);

        result.Should().Be(90m);
    }

    [Fact]
    public async Task CalculateFinalScore_MinFormula()
    {
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessRule>
            {
                new() { Id = 1, OrgUnitId = 1, RuleType = Domain.Enums.BusinessRuleType.ScoreCalculation, ActionParameters = "Min", IsActive = true, Priority = 1 }
            });

        var result = await _sut.CalculateFinalScore(80m, 90m, 1);

        result.Should().Be(80m);
    }

    [Fact]
    public async Task CalculateFinalScore_WeightedFirstFormula()
    {
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessRule>
            {
                new() { Id = 1, OrgUnitId = 1, RuleType = Domain.Enums.BusinessRuleType.ScoreCalculation, ActionParameters = "WeightedFirst", IsActive = true, Priority = 1 }
            });

        var result = await _sut.CalculateFinalScore(80m, 90m, 1);

        result.Should().Be(80m * 0.6m + 90m * 0.4m); // 48 + 36 = 84
    }

    #endregion

    #region Submit Appeal

    [Fact]
    public async Task SubmitAppealAsync_ValidAppeal_UpdatesScoreAndRecalculates()
    {
        var exam = new Exam
        {
            Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m,
            PassingScore = 60m, AppealDeadline = DateTime.UtcNow.AddDays(10)
        };
        var existingScore = new ExamScore
        {
            Id = 50, ExamId = 1, CandidacyId = 5,
            FirstExaminerScore = 40m, SecondExaminerScore = 50m,
            FinalScore = 45m, PassedThreshold = false
        };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExamScore> { existingScore });
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BusinessRule>());
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });

        var command = new SubmitAppealCommand(1, 5, 80m, "ערעור על ציון");

        var result = await _sut.SubmitAppealAsync(command);

        result.IsAppealed.Should().BeTrue();
        result.AppealScore.Should().Be(80m);
        // Appeal score (80) replaces the lower score (40), so final = (80 + 50) / 2 = 65
        result.FinalScore.Should().Be(65m);
        result.PassedThreshold.Should().BeTrue();
        _scoreRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ExamScore>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAppealAsync_NoAppealDeadline_ThrowsValidationException()
    {
        var exam = new Exam
        {
            Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m,
            AppealDeadline = null
        };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        var command = new SubmitAppealCommand(1, 5, 80m, "ערעור");

        var act = () => _sut.SubmitAppealAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAppealAsync_DeadlinePassed_ThrowsValidationException()
    {
        var exam = new Exam
        {
            Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m,
            AppealDeadline = DateTime.UtcNow.AddDays(-1)
        };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        var command = new SubmitAppealCommand(1, 5, 80m, "ערעור");

        var act = () => _sut.SubmitAppealAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAppealAsync_ScoreNotFound_ThrowsNotFoundException()
    {
        var exam = new Exam
        {
            Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m,
            AppealDeadline = DateTime.UtcNow.AddDays(10)
        };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ExamScore>());

        var command = new SubmitAppealCommand(1, 5, 80m, "ערעור");

        var act = () => _sut.SubmitAppealAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SubmitAppealAsync_AppealScoreExceedsMax_ThrowsValidationException()
    {
        var exam = new Exam
        {
            Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m,
            AppealDeadline = DateTime.UtcNow.AddDays(10)
        };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        var command = new SubmitAppealCommand(1, 5, 150m, "ערעור");

        var act = () => _sut.SubmitAppealAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAppealAsync_AppealPassesThreshold_UpdatesCandidacyStatus()
    {
        var exam = new Exam
        {
            Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, MaxScore = 100m,
            PassingScore = 60m, AppealDeadline = DateTime.UtcNow.AddDays(10)
        };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, CurrentStatusId = 20, IsActive = true };
        var existingScore = new ExamScore
        {
            Id = 50, ExamId = 1, CandidacyId = 5,
            FirstExaminerScore = 40m, SecondExaminerScore = 50m,
            FinalScore = 45m, PassedThreshold = false
        };
        var passedStatus = new StatusDefinition { Id = 30, OrgUnitId = 1, Code = "passed_exam", IsFinal = false };

        _examRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExamScore> { existingScore });
        _businessRuleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BusinessRule, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<BusinessRule>());
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition> { passedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusTransition> { new() { Id = 1, FromStatusId = 20, ToStatusId = 30, OrgUnitId = 1 } });

        var command = new SubmitAppealCommand(1, 5, 80m, "ערעור על ציון");

        var result = await _sut.SubmitAppealAsync(command);

        result.PassedThreshold.Should().BeTrue();
        candidacy.CurrentStatusId.Should().Be(30);
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAppealAsync_ExamNotFound_ThrowsNotFoundException()
    {
        _examRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);

        var command = new SubmitAppealCommand(999, 5, 80m, "ערעור");

        var act = () => _sut.SubmitAppealAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Get Score

    [Fact]
    public async Task GetScoreAsync_NotFound_ThrowsNotFoundException()
    {
        _scoreRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExamScore, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ExamScore>());

        var act = () => _sut.GetScoreAsync(1, 5);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetScoresByExamAsync_ExamNotFound_ThrowsNotFoundException()
    {
        _examRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);

        var act = () => _sut.GetScoresByExamAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
