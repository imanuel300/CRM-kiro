using System.Linq.Expressions;
using CandidacyManagement.Application.Tenures;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Tenures;

public class TenureServiceTests
{
    private readonly Mock<IRepository<Tenure>> _tenureRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly TenureService _sut;

    public TenureServiceTests()
    {
        _tenureRepoMock = new Mock<IRepository<Tenure>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();

        _sut = new TenureService(
            _tenureRepoMock.Object,
            _contactRepoMock.Object,
            _orgUnitRepoMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsTenureDto()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _tenureRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenure, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Tenure>());
        _tenureRepoMock.Setup(r => r.AddAsync(It.IsAny<Tenure>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenure e, CancellationToken _) => { e.Id = 10; return e; });

        var command = new CreateTenureCommand(
            ContactId: 1, OrgUnitId: 1, Position: "שופט",
            StartDate: new DateTime(2024, 1, 1),
            ExpectedEndDate: new DateTime(2027, 1, 1),
            Notes: "כהונה ראשונה");

        var result = await _sut.CreateAsync(command);

        result.Should().NotBeNull();
        result.ContactId.Should().Be(1);
        result.OrgUnitId.Should().Be(1);
        result.Position.Should().Be("שופט");
        result.IsActive.Should().BeTrue();
        result.Notes.Should().Be("כהונה ראשונה");
    }

    [Fact]
    public async Task CreateAsync_ContactNotFound_ThrowsNotFoundException()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var command = new CreateTenureCommand(
            ContactId: 999, OrgUnitId: 1, Position: "שופט",
            StartDate: DateTime.UtcNow, ExpectedEndDate: DateTime.UtcNow.AddYears(3), Notes: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var command = new CreateTenureCommand(
            ContactId: 1, OrgUnitId: 999, Position: "שופט",
            StartDate: DateTime.UtcNow, ExpectedEndDate: DateTime.UtcNow.AddYears(3), Notes: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_EmptyPosition_ThrowsValidationException()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateTenureCommand(
            ContactId: 1, OrgUnitId: 1, Position: "",
            StartDate: DateTime.UtcNow, ExpectedEndDate: DateTime.UtcNow.AddYears(3), Notes: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_EndDateBeforeStartDate_ThrowsValidationException()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateTenureCommand(
            ContactId: 1, OrgUnitId: 1, Position: "שופט",
            StartDate: new DateTime(2025, 1, 1),
            ExpectedEndDate: new DateTime(2024, 1, 1),
            Notes: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateActiveTenure_ThrowsValidationException()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _tenureRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenure, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenure> { new() { Id = 5, IsActive = true } });

        var command = new CreateTenureCommand(
            ContactId: 1, OrgUnitId: 1, Position: "שופט",
            StartDate: DateTime.UtcNow, ExpectedEndDate: DateTime.UtcNow.AddYears(3), Notes: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsTenureDto()
    {
        var entity = new Tenure
        {
            Id = 10, ContactId = 1, OrgUnitId = 1, Position = "שופט",
            StartDate = new DateTime(2024, 1, 1), ExpectedEndDate = new DateTime(2027, 1, 1),
            IsActive = true
        };
        _tenureRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new UpdateTenureCommand(
            Id: 10, Position: "שופט בכיר",
            StartDate: new DateTime(2024, 1, 1),
            ExpectedEndDate: new DateTime(2028, 1, 1),
            Notes: "עודכן");

        var result = await _sut.UpdateAsync(command);

        result.Should().NotBeNull();
        result.Position.Should().Be("שופט בכיר");
        result.Notes.Should().Be("עודכן");
    }

    [Fact]
    public async Task UpdateAsync_InactiveTenure_ThrowsValidationException()
    {
        var entity = new Tenure { Id = 10, IsActive = false };
        _tenureRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new UpdateTenureCommand(
            Id: 10, Position: "שופט",
            StartDate: DateTime.UtcNow, ExpectedEndDate: DateTime.UtcNow.AddYears(3), Notes: null);

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        _tenureRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenure?)null);

        var command = new UpdateTenureCommand(
            Id: 999, Position: "שופט",
            StartDate: DateTime.UtcNow, ExpectedEndDate: DateTime.UtcNow.AddYears(3), Notes: null);

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region EndTenureAsync

    [Fact]
    public async Task EndTenureAsync_WithValidCommand_ReturnsTenureDto()
    {
        var entity = new Tenure
        {
            Id = 10, ContactId = 1, OrgUnitId = 1, Position = "שופט",
            StartDate = new DateTime(2024, 1, 1), ExpectedEndDate = new DateTime(2027, 1, 1),
            IsActive = true
        };
        _tenureRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new EndTenureCommand(
            Id: 10, EndReason: TenureEndReason.TermExpired,
            ActualEndDate: new DateTime(2027, 1, 1), Notes: "סיום תקופה");

        var result = await _sut.EndTenureAsync(command);

        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
        result.EndReason.Should().Be(TenureEndReason.TermExpired);
        result.ActualEndDate.Should().Be(new DateTime(2027, 1, 1));
    }

    [Fact]
    public async Task EndTenureAsync_AlreadyEnded_ThrowsValidationException()
    {
        var entity = new Tenure { Id = 10, IsActive = false };
        _tenureRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new EndTenureCommand(
            Id: 10, EndReason: TenureEndReason.Resignation,
            ActualEndDate: DateTime.UtcNow, Notes: null);

        var act = () => _sut.EndTenureAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task EndTenureAsync_Resignation_SetsCorrectEndReason()
    {
        var entity = new Tenure
        {
            Id = 10, ContactId = 1, OrgUnitId = 1, Position = "נציג ציבור",
            StartDate = new DateTime(2024, 1, 1), ExpectedEndDate = new DateTime(2027, 1, 1),
            IsActive = true
        };
        _tenureRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new EndTenureCommand(
            Id: 10, EndReason: TenureEndReason.Resignation,
            ActualEndDate: new DateTime(2025, 6, 15), Notes: "התפטרות מרצון");

        var result = await _sut.EndTenureAsync(command);

        result.EndReason.Should().Be(TenureEndReason.Resignation);
        result.Notes.Should().Be("התפטרות מרצון");
    }

    [Fact]
    public async Task EndTenureAsync_NoActualEndDate_UsesUtcNow()
    {
        var entity = new Tenure
        {
            Id = 10, ContactId = 1, OrgUnitId = 1, Position = "שופט",
            StartDate = new DateTime(2024, 1, 1), ExpectedEndDate = new DateTime(2027, 1, 1),
            IsActive = true
        };
        _tenureRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new EndTenureCommand(
            Id: 10, EndReason: TenureEndReason.Termination,
            ActualEndDate: null, Notes: null);

        var result = await _sut.EndTenureAsync(command);

        result.ActualEndDate.Should().NotBeNull();
        result.ActualEndDate!.Value.Date.Should().Be(DateTime.UtcNow.Date);
    }

    #endregion

    #region GetByContactAsync

    [Fact]
    public async Task GetByContactAsync_ReturnsAllTenuresForContact()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1 });
        _tenureRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenure, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenure>
            {
                new() { Id = 10, ContactId = 1, OrgUnitId = 1, Position = "שופט", StartDate = DateTime.UtcNow, ExpectedEndDate = DateTime.UtcNow.AddYears(3), IsActive = true },
                new() { Id = 11, ContactId = 1, OrgUnitId = 2, Position = "נציג", StartDate = DateTime.UtcNow.AddYears(-3), ExpectedEndDate = DateTime.UtcNow, IsActive = false }
            });

        var result = await _sut.GetByContactAsync(1);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByContactAsync_ContactNotFound_ThrowsNotFoundException()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.GetByContactAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetByOrgUnitAsync

    [Fact]
    public async Task GetByOrgUnitAsync_ReturnsAllTenuresForOrgUnit()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _tenureRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenure, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenure>
            {
                new() { Id = 10, ContactId = 1, OrgUnitId = 1, Position = "שופט", StartDate = DateTime.UtcNow, ExpectedEndDate = DateTime.UtcNow.AddYears(3), IsActive = true }
            });

        var result = await _sut.GetByOrgUnitAsync(1);

        result.Should().HaveCount(1);
    }

    #endregion

    #region GetExpiringTenuresAsync

    [Fact]
    public async Task GetExpiringTenuresAsync_ReturnsExpiringTenures()
    {
        var expiringTenure = new Tenure
        {
            Id = 10, ContactId = 1, OrgUnitId = 1, Position = "שופט",
            StartDate = DateTime.UtcNow.AddYears(-3),
            ExpectedEndDate = DateTime.UtcNow.AddDays(15),
            IsActive = true
        };
        _tenureRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenure, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenure> { expiringTenure });

        var result = (await _sut.GetExpiringTenuresAsync(30)).ToList();

        result.Should().HaveCount(1);
        result[0].DaysUntilExpiry.Should().BeLessThanOrEqualTo(30);
    }

    [Fact]
    public async Task GetExpiringTenuresAsync_NoExpiring_ReturnsEmpty()
    {
        _tenureRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenure, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Tenure>());

        var result = await _sut.GetExpiringTenuresAsync(30);

        result.Should().BeEmpty();
    }

    #endregion

    #region GetHistoryAsync

    [Fact]
    public async Task GetHistoryAsync_ReturnsOnlyInactiveTenures()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 1 });
        _tenureRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Tenure, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenure>
            {
                new() { Id = 11, ContactId = 1, OrgUnitId = 1, Position = "שופט", StartDate = DateTime.UtcNow.AddYears(-6), ExpectedEndDate = DateTime.UtcNow.AddYears(-3), ActualEndDate = DateTime.UtcNow.AddYears(-3), EndReason = TenureEndReason.TermExpired, IsActive = false }
            });

        var result = (await _sut.GetHistoryAsync(1)).ToList();

        result.Should().HaveCount(1);
        result[0].IsActive.Should().BeFalse();
        result[0].EndReason.Should().Be(TenureEndReason.TermExpired);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingTenure_DeletesSuccessfully()
    {
        var entity = new Tenure { Id = 10 };
        _tenureRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _sut.DeleteAsync(10);

        _tenureRepoMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _tenureRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenure?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
