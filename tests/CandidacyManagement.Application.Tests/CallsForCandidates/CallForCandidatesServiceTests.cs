using System.Linq.Expressions;
using CandidacyManagement.Application.CallsForCandidates;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.CallsForCandidates;

public class CallForCandidatesServiceTests
{
    private readonly Mock<IRepository<CallForCandidates>> _callRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<ThresholdCondition>> _thresholdRepoMock;
    private readonly Mock<IRepository<Position>> _positionRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly CallForCandidatesService _sut;

    public CallForCandidatesServiceTests()
    {
        _callRepoMock = new Mock<IRepository<CallForCandidates>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _thresholdRepoMock = new Mock<IRepository<ThresholdCondition>>();
        _positionRepoMock = new Mock<IRepository<Position>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();

        _sut = new CallForCandidatesService(
            _callRepoMock.Object,
            _orgUnitRepoMock.Object,
            _thresholdRepoMock.Object,
            _positionRepoMock.Object,
            _candidacyRepoMock.Object,
            _statusRepoMock.Object);
    }

    #region Create

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsDto()
    {
        // Arrange
        var command = new CreateCallForCandidatesCommand(
            OrgUnitId: 1, Title: "קול קורא לעוזרים משפטיים",
            Description: "תיאור", OpenDate: DateTime.UtcNow,
            CloseDate: DateTime.UtcNow.AddDays(30),
            IsTender: false, MinScore: null, EligibilityConditions: null);

        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "עוזמת" });
        _callRepoMock.Setup(r => r.AddAsync(It.IsAny<CallForCandidates>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates e, CancellationToken _) => { e.Id = 10; return e; });

        // Act
        var result = await _sut.CreateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("קול קורא לעוזרים משפטיים");
        result.OrgUnitId.Should().Be(1);
        result.IsActive.Should().BeTrue();
        result.IsTender.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_AsTender_SetsTenderFields()
    {
        var command = new CreateCallForCandidatesCommand(
            OrgUnitId: 1, Title: "מכרז לנציגי ציבור",
            Description: "מכרז", OpenDate: DateTime.UtcNow,
            CloseDate: DateTime.UtcNow.AddDays(60),
            IsTender: true, MinScore: 75.5m, EligibilityConditions: "תואר ראשון");

        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "נציגי ציבור" });
        _callRepoMock.Setup(r => r.AddAsync(It.IsAny<CallForCandidates>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates e, CancellationToken _) => { e.Id = 11; return e; });

        var result = await _sut.CreateAsync(command);

        result.IsTender.Should().BeTrue();
        result.MinScore.Should().Be(75.5m);
        result.EligibilityConditions.Should().Be("תואר ראשון");
    }

    [Fact]
    public async Task CreateAsync_NonTender_ClearsTenderFields()
    {
        var command = new CreateCallForCandidatesCommand(
            OrgUnitId: 1, Title: "קול קורא רגיל",
            Description: null, OpenDate: DateTime.UtcNow,
            CloseDate: DateTime.UtcNow.AddDays(30),
            IsTender: false, MinScore: 80m, EligibilityConditions: "should be cleared");

        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "test" });
        _callRepoMock.Setup(r => r.AddAsync(It.IsAny<CallForCandidates>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates e, CancellationToken _) => { e.Id = 12; return e; });

        var result = await _sut.CreateAsync(command);

        result.MinScore.Should().BeNull();
        result.EligibilityConditions.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTitle_ThrowsValidationException()
    {
        var command = new CreateCallForCandidatesCommand(
            OrgUnitId: 1, Title: "  ",
            Description: null, OpenDate: DateTime.UtcNow,
            CloseDate: null, IsTender: false, MinScore: null, EligibilityConditions: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_WithCloseDateBeforeOpenDate_ThrowsValidationException()
    {
        var command = new CreateCallForCandidatesCommand(
            OrgUnitId: 1, Title: "קול קורא",
            Description: null, OpenDate: DateTime.UtcNow.AddDays(10),
            CloseDate: DateTime.UtcNow,
            IsTender: false, MinScore: null, EligibilityConditions: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentOrgUnit_ThrowsNotFoundException()
    {
        var command = new CreateCallForCandidatesCommand(
            OrgUnitId: 999, Title: "קול קורא",
            Description: null, OpenDate: DateTime.UtcNow,
            CloseDate: null, IsTender: false, MinScore: null, EligibilityConditions: null);

        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_TenderWithNegativeMinScore_ThrowsValidationException()
    {
        var command = new CreateCallForCandidatesCommand(
            OrgUnitId: 1, Title: "מכרז",
            Description: null, OpenDate: DateTime.UtcNow,
            CloseDate: DateTime.UtcNow.AddDays(30),
            IsTender: true, MinScore: -5m, EligibilityConditions: null);

        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "test" });

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsUpdatedDto()
    {
        var existing = CreateCallEntity(1);
        var command = new UpdateCallForCandidatesCommand(
            Id: 1, Title: "כותרת מעודכנת",
            Description: "תיאור חדש", OpenDate: DateTime.UtcNow,
            CloseDate: DateTime.UtcNow.AddDays(45),
            IsTender: false, MinScore: null, EligibilityConditions: null);

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(command);

        result.Title.Should().Be("כותרת מעודכנת");
        result.Description.Should().Be("תיאור חדש");
        _callRepoMock.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentId_ThrowsNotFoundException()
    {
        var command = new UpdateCallForCandidatesCommand(
            Id: 999, Title: "כותרת",
            Description: null, OpenDate: DateTime.UtcNow,
            CloseDate: null, IsTender: false, MinScore: null, EligibilityConditions: null);

        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ToTender_SetsTenderFields()
    {
        var existing = CreateCallEntity(1, isTender: false);
        var command = new UpdateCallForCandidatesCommand(
            Id: 1, Title: "מכרז חדש",
            Description: null, OpenDate: DateTime.UtcNow,
            CloseDate: DateTime.UtcNow.AddDays(30),
            IsTender: true, MinScore: 80m, EligibilityConditions: "ניסיון 3 שנים");

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(command);

        result.IsTender.Should().BeTrue();
        result.MinScore.Should().Be(80m);
        result.EligibilityConditions.Should().Be("ניסיון 3 שנים");
    }

    #endregion

    #region GetById and GetDetail

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDto()
    {
        var entity = CreateCallEntity(5, title: "קול קורא קיים");
        _callRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(5);

        result.Id.Should().Be(5);
        result.Title.Should().Be("קול קורא קיים");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ThrowsNotFoundException()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsDetailWithThresholdsAndPositions()
    {
        var entity = CreateCallEntity(3);
        _callRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _thresholdRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCondition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ThresholdCondition>
            {
                new() { Id = 1, CallForCandidatesId = 3, FieldName = "Age", Operator = ">=", Value = "21", IsAutomatic = true }
            });
        _positionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Id = 1, CallForCandidatesId = 3, Title = "עוזר משפטי", Vacancies = 2 }
            });

        var result = await _sut.GetDetailAsync(3);

        result.ThresholdConditions.Should().HaveCount(1);
        result.Positions.Should().HaveCount(1);
        result.ThresholdConditions.First().FieldName.Should().Be("Age");
        result.Positions.First().Title.Should().Be("עוזר משפטי");
    }

    #endregion

    #region List

    [Fact]
    public async Task ListAsync_WithOrgUnitFilter_ReturnsFilteredResults()
    {
        var calls = new List<CallForCandidates>
        {
            CreateCallEntity(1, orgUnitId: 1),
            CreateCallEntity(2, orgUnitId: 1)
        };
        _callRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CallForCandidates, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(calls);

        var result = await _sut.ListAsync(new CallForCandidatesQueryParams(OrgUnitId: 1));

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListAsync_WithTenderFilter_ReturnsOnlyTenders()
    {
        var calls = new List<CallForCandidates>
        {
            CreateCallEntity(1, isTender: true)
        };
        _callRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CallForCandidates, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(calls);

        var result = await _sut.ListAsync(new CallForCandidatesQueryParams(IsTender: true));

        result.Should().HaveCount(1);
        result.First().IsTender.Should().BeTrue();
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesEntity()
    {
        var entity = CreateCallEntity(7);
        _callRepoMock.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _sut.DeleteAsync(7);

        _callRepoMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ThrowsNotFoundException()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Threshold Conditions

    [Fact]
    public async Task AddThresholdConditionAsync_WithValidCommand_ReturnsDto()
    {
        var command = new CreateThresholdConditionCommand(
            CallForCandidatesId: 1, FieldName: "Age",
            Operator: ">=", Value: "21", IsAutomatic: true);

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));
        _thresholdRepoMock.Setup(r => r.AddAsync(It.IsAny<ThresholdCondition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCondition e, CancellationToken _) => { e.Id = 100; return e; });

        var result = await _sut.AddThresholdConditionAsync(command);

        result.FieldName.Should().Be("Age");
        result.Operator.Should().Be(">=");
        result.Value.Should().Be("21");
        result.IsAutomatic.Should().BeTrue();
    }

    [Fact]
    public async Task AddThresholdConditionAsync_NonExistentCall_ThrowsNotFoundException()
    {
        var command = new CreateThresholdConditionCommand(
            CallForCandidatesId: 999, FieldName: "Age",
            Operator: ">=", Value: "21", IsAutomatic: true);

        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.AddThresholdConditionAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddThresholdConditionAsync_EmptyFieldName_ThrowsValidationException()
    {
        var command = new CreateThresholdConditionCommand(
            CallForCandidatesId: 1, FieldName: "",
            Operator: ">=", Value: "21", IsAutomatic: true);

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));

        var act = () => _sut.AddThresholdConditionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AddThresholdConditionAsync_EmptyOperator_ThrowsValidationException()
    {
        var command = new CreateThresholdConditionCommand(
            CallForCandidatesId: 1, FieldName: "Age",
            Operator: "", Value: "21", IsAutomatic: true);

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));

        var act = () => _sut.AddThresholdConditionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AddThresholdConditionAsync_EmptyValue_ThrowsValidationException()
    {
        var command = new CreateThresholdConditionCommand(
            CallForCandidatesId: 1, FieldName: "Age",
            Operator: ">=", Value: "  ", IsAutomatic: true);

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));

        var act = () => _sut.AddThresholdConditionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RemoveThresholdConditionAsync_ExistingId_DeletesCondition()
    {
        var condition = new ThresholdCondition { Id = 10, CallForCandidatesId = 1, FieldName = "Age", Operator = ">=", Value = "21" };
        _thresholdRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(condition);

        await _sut.RemoveThresholdConditionAsync(10);

        _thresholdRepoMock.Verify(r => r.DeleteAsync(condition, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveThresholdConditionAsync_NonExistentId_ThrowsNotFoundException()
    {
        _thresholdRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCondition?)null);

        var act = () => _sut.RemoveThresholdConditionAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetThresholdConditionsAsync_ReturnsConditionsForCall()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));
        _thresholdRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCondition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ThresholdCondition>
            {
                new() { Id = 1, CallForCandidatesId = 1, FieldName = "Age", Operator = ">=", Value = "21", IsAutomatic = true },
                new() { Id = 2, CallForCandidatesId = 1, FieldName = "Education", Operator = "==", Value = "BA", IsAutomatic = false }
            });

        var result = await _sut.GetThresholdConditionsAsync(1);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetThresholdConditionsAsync_NonExistentCall_ThrowsNotFoundException()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.GetThresholdConditionsAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Positions

    [Fact]
    public async Task AddPositionAsync_WithValidCommand_ReturnsDto()
    {
        var command = new CreatePositionCommand(
            CallForCandidatesId: 1, Title: "עוזר משפטי",
            Description: "עוזר משפטי בלשכת שופט", Vacancies: 3);

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));
        _positionRepoMock.Setup(r => r.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position e, CancellationToken _) => { e.Id = 50; return e; });

        var result = await _sut.AddPositionAsync(command);

        result.Title.Should().Be("עוזר משפטי");
        result.Description.Should().Be("עוזר משפטי בלשכת שופט");
        result.Vacancies.Should().Be(3);
    }

    [Fact]
    public async Task AddPositionAsync_NonExistentCall_ThrowsNotFoundException()
    {
        var command = new CreatePositionCommand(
            CallForCandidatesId: 999, Title: "תפקיד",
            Description: null, Vacancies: 1);

        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.AddPositionAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddPositionAsync_EmptyTitle_ThrowsValidationException()
    {
        var command = new CreatePositionCommand(
            CallForCandidatesId: 1, Title: "",
            Description: null, Vacancies: 1);

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));

        var act = () => _sut.AddPositionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AddPositionAsync_ZeroVacancies_ThrowsValidationException()
    {
        var command = new CreatePositionCommand(
            CallForCandidatesId: 1, Title: "תפקיד",
            Description: null, Vacancies: 0);

        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));

        var act = () => _sut.AddPositionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RemovePositionAsync_ExistingId_DeletesPosition()
    {
        var position = new Position { Id = 20, CallForCandidatesId = 1, Title = "תפקיד", Vacancies = 1 };
        _positionRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        await _sut.RemovePositionAsync(20);

        _positionRepoMock.Verify(r => r.DeleteAsync(position, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemovePositionAsync_NonExistentId_ThrowsNotFoundException()
    {
        _positionRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        var act = () => _sut.RemovePositionAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPositionsAsync_ReturnsPositionsForCall()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCallEntity(1));
        _positionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Position>
            {
                new() { Id = 1, CallForCandidatesId = 1, Title = "עוזר משפטי", Vacancies = 2 },
                new() { Id = 2, CallForCandidatesId = 1, Title = "מזכיר", Vacancies = 1 }
            });

        var result = await _sut.GetPositionsAsync(1);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPositionsAsync_NonExistentCall_ThrowsNotFoundException()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.GetPositionsAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Closing Logic - IsClosedAsync

    [Fact]
    public async Task IsClosedAsync_CloseDateInPast_ReturnsTrue()
    {
        var entity = CreateCallEntity(1);
        entity.CloseDate = DateTime.UtcNow.AddDays(-1);
        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.IsClosedAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsClosedAsync_CloseDateInFuture_ReturnsFalse()
    {
        var entity = CreateCallEntity(1);
        entity.CloseDate = DateTime.UtcNow.AddDays(10);
        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.IsClosedAsync(1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsClosedAsync_NoCloseDate_ReturnsFalse()
    {
        var entity = CreateCallEntity(1);
        entity.CloseDate = null;
        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.IsClosedAsync(1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsClosedAsync_NonExistentId_ThrowsNotFoundException()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.IsClosedAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Closing Logic - GetClosingSummaryAsync

    [Fact]
    public async Task GetClosingSummaryAsync_ReturnsCorrectCounts()
    {
        var entity = CreateCallEntity(1, title: "קול קורא לסיכום");
        entity.CloseDate = DateTime.UtcNow.AddDays(-1);
        _callRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var candidacies = new List<Candidacy>
        {
            new() { Id = 1, CallForCandidatesId = 1, CurrentStatusId = 10, IsActive = true },
            new() { Id = 2, CallForCandidatesId = 1, CurrentStatusId = 20, IsActive = true },
            new() { Id = 3, CallForCandidatesId = 1, CurrentStatusId = 30, IsActive = false }
        };
        _candidacyRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacies);

        // Status 10 = Submitted, Status 20 = InReview (met threshold), Status 30 = Rejected
        _statusRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 10, Code = "submitted", Category = CandidacyStatusCategory.Submitted });
        _statusRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 20, Code = "in_review", Category = CandidacyStatusCategory.InReview });
        _statusRepoMock.Setup(r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 30, Code = "rejected", Category = CandidacyStatusCategory.Rejected });

        var result = await _sut.GetClosingSummaryAsync(1);

        result.TotalCandidacies.Should().Be(3);
        result.MetThreshold.Should().Be(1); // Only InReview counts
        result.Rejected.Should().Be(1);
        result.Title.Should().Be("קול קורא לסיכום");
    }

    [Fact]
    public async Task GetClosingSummaryAsync_NoCandidacies_ReturnsZeroCounts()
    {
        var entity = CreateCallEntity(2, title: "קול קורא ריק");
        _callRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _candidacyRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Candidacy>());

        var result = await _sut.GetClosingSummaryAsync(2);

        result.TotalCandidacies.Should().Be(0);
        result.MetThreshold.Should().Be(0);
        result.Rejected.Should().Be(0);
    }

    [Fact]
    public async Task GetClosingSummaryAsync_NonExistentId_ThrowsNotFoundException()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.GetClosingSummaryAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetClosingSummaryAsync_AllRejected_CountsCorrectly()
    {
        var entity = CreateCallEntity(3);
        _callRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var candidacies = new List<Candidacy>
        {
            new() { Id = 1, CallForCandidatesId = 3, CurrentStatusId = 30, IsActive = false },
            new() { Id = 2, CallForCandidatesId = 3, CurrentStatusId = 30, IsActive = false }
        };
        _candidacyRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacies);
        _statusRepoMock.Setup(r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition { Id = 30, Code = "rejected", Category = CandidacyStatusCategory.Rejected });

        var result = await _sut.GetClosingSummaryAsync(3);

        result.TotalCandidacies.Should().Be(2);
        result.MetThreshold.Should().Be(0);
        result.Rejected.Should().Be(2);
    }

    #endregion

    #region Helpers

    private static CallForCandidates CreateCallEntity(
        int id, int orgUnitId = 1, string title = "קול קורא", bool isTender = false) =>
        new()
        {
            Id = id,
            OrgUnitId = orgUnitId,
            Title = title,
            Description = "תיאור",
            OpenDate = DateTime.UtcNow,
            CloseDate = DateTime.UtcNow.AddDays(30),
            IsTender = isTender,
            MinScore = isTender ? 70m : null,
            EligibilityConditions = isTender ? "תנאי כשירות" : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    #endregion
}
