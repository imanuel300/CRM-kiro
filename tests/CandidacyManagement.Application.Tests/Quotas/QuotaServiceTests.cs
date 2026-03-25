using System.Linq.Expressions;
using CandidacyManagement.Application.Quotas;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Quotas;

public class QuotaServiceTests
{
    private readonly Mock<IRepository<Quota>> _quotaRepoMock;
    private readonly Mock<IRepository<QuotaAssignment>> _assignmentRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly QuotaService _sut;

    public QuotaServiceTests()
    {
        _quotaRepoMock = new Mock<IRepository<Quota>>();
        _assignmentRepoMock = new Mock<IRepository<QuotaAssignment>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();

        _sut = new QuotaService(
            _quotaRepoMock.Object,
            _assignmentRepoMock.Object,
            _orgUnitRepoMock.Object,
            _candidacyRepoMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsQuotaDto()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _quotaRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Quota, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Quota>());
        _quotaRepoMock.Setup(r => r.AddAsync(It.IsAny<Quota>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Quota e, CancellationToken _) => { e.Id = 10; return e; });

        var command = new CreateQuotaCommand(
            OrgUnitId: 1, CategoryName: "מגזר ערבי", TargetCount: 5, Description: "ייצוג הולם");

        var result = await _sut.CreateAsync(command);

        result.Should().NotBeNull();
        result.OrgUnitId.Should().Be(1);
        result.CategoryName.Should().Be("מגזר ערבי");
        result.TargetCount.Should().Be(5);
        result.CurrentCount.Should().Be(0);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var command = new CreateQuotaCommand(OrgUnitId: 999, CategoryName: "נשים", TargetCount: 3, Description: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_EmptyCategoryName_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateQuotaCommand(OrgUnitId: 1, CategoryName: "", TargetCount: 3, Description: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ZeroTargetCount_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateQuotaCommand(OrgUnitId: 1, CategoryName: "נשים", TargetCount: 0, Description: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateActiveCategory_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _quotaRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Quota, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Quota> { new() { Id = 5, CategoryName = "נשים", IsActive = true } });

        var command = new CreateQuotaCommand(OrgUnitId: 1, CategoryName: "נשים", TargetCount: 3, Description: null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsUpdatedQuotaDto()
    {
        var entity = new Quota
        {
            Id = 10, OrgUnitId = 1, CategoryName = "נשים",
            TargetCount = 3, CurrentCount = 1, IsActive = true
        };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new UpdateQuotaCommand(
            Id: 10, CategoryName: "נשים", TargetCount: 5, Description: "עודכן", IsActive: true);

        var result = await _sut.UpdateAsync(command);

        result.Should().NotBeNull();
        result.TargetCount.Should().Be(5);
        result.Description.Should().Be("עודכן");
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        _quotaRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Quota?)null);

        var command = new UpdateQuotaCommand(Id: 999, CategoryName: "נשים", TargetCount: 3, Description: null, IsActive: true);

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_EmptyCategoryName_ThrowsValidationException()
    {
        var entity = new Quota { Id = 10, IsActive = true };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new UpdateQuotaCommand(Id: 10, CategoryName: "  ", TargetCount: 3, Description: null, IsActive: true);

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_NegativeTargetCount_ThrowsValidationException()
    {
        var entity = new Quota { Id = 10, IsActive = true };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new UpdateQuotaCommand(Id: 10, CategoryName: "נשים", TargetCount: -1, Description: null, IsActive: true);

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingQuota_DeletesSuccessfully()
    {
        var entity = new Quota { Id = 10 };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _sut.DeleteAsync(10);

        _quotaRepoMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _quotaRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Quota?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingQuota_ReturnsDto()
    {
        var entity = new Quota
        {
            Id = 10, OrgUnitId = 1, CategoryName = "מגזר ערבי",
            TargetCount = 5, CurrentCount = 2, IsActive = true
        };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(10);

        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.CategoryName.Should().Be("מגזר ערבי");
    }

    #endregion

    #region GetByOrgUnitAsync

    [Fact]
    public async Task GetByOrgUnitAsync_ReturnsAllQuotasForOrgUnit()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _quotaRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Quota, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Quota>
            {
                new() { Id = 10, OrgUnitId = 1, CategoryName = "נשים", TargetCount = 3, CurrentCount = 1, IsActive = true },
                new() { Id = 11, OrgUnitId = 1, CategoryName = "מגזר ערבי", TargetCount = 5, CurrentCount = 2, IsActive = true }
            });

        var result = await _sut.GetByOrgUnitAsync(1);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByOrgUnitAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var act = () => _sut.GetByOrgUnitAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region AssignCandidacyAsync

    [Fact]
    public async Task AssignCandidacyAsync_WithValidCommand_ReturnsAssignmentAndIncrementsCount()
    {
        var quota = new Quota
        {
            Id = 10, OrgUnitId = 1, CategoryName = "נשים",
            TargetCount = 3, CurrentCount = 1, IsActive = true
        };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quota);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 100 });
        _assignmentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<QuotaAssignment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<QuotaAssignment>());
        _assignmentRepoMock.Setup(r => r.AddAsync(It.IsAny<QuotaAssignment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuotaAssignment e, CancellationToken _) => { e.Id = 1; return e; });

        var command = new AssignCandidacyCommand(QuotaId: 10, CandidacyId: 100);

        var result = await _sut.AssignCandidacyAsync(command);

        result.Should().NotBeNull();
        result.QuotaId.Should().Be(10);
        result.CandidacyId.Should().Be(100);
        quota.CurrentCount.Should().Be(2);
    }

    [Fact]
    public async Task AssignCandidacyAsync_InactiveQuota_ThrowsValidationException()
    {
        var quota = new Quota { Id = 10, IsActive = false };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quota);

        var command = new AssignCandidacyCommand(QuotaId: 10, CandidacyId: 100);

        var act = () => _sut.AssignCandidacyAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AssignCandidacyAsync_DuplicateAssignment_ThrowsValidationException()
    {
        var quota = new Quota { Id = 10, IsActive = true };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quota);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 100 });
        _assignmentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<QuotaAssignment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<QuotaAssignment> { new() { Id = 1, QuotaId = 10, CandidacyId = 100 } });

        var command = new AssignCandidacyCommand(QuotaId: 10, CandidacyId: 100);

        var act = () => _sut.AssignCandidacyAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AssignCandidacyAsync_QuotaNotFound_ThrowsNotFoundException()
    {
        _quotaRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Quota?)null);

        var command = new AssignCandidacyCommand(QuotaId: 999, CandidacyId: 100);

        var act = () => _sut.AssignCandidacyAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignCandidacyAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        var quota = new Quota { Id = 10, IsActive = true };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quota);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var command = new AssignCandidacyCommand(QuotaId: 10, CandidacyId: 999);

        var act = () => _sut.AssignCandidacyAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UnassignCandidacyAsync

    [Fact]
    public async Task UnassignCandidacyAsync_WithValidCommand_DecrementsCount()
    {
        var quota = new Quota
        {
            Id = 10, OrgUnitId = 1, CategoryName = "נשים",
            TargetCount = 3, CurrentCount = 2, IsActive = true
        };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quota);
        _assignmentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<QuotaAssignment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<QuotaAssignment> { new() { Id = 1, QuotaId = 10, CandidacyId = 100 } });

        var command = new UnassignCandidacyCommand(QuotaId: 10, CandidacyId: 100);

        await _sut.UnassignCandidacyAsync(command);

        quota.CurrentCount.Should().Be(1);
        _assignmentRepoMock.Verify(r => r.DeleteAsync(It.IsAny<QuotaAssignment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnassignCandidacyAsync_AssignmentNotFound_ThrowsNotFoundException()
    {
        var quota = new Quota { Id = 10, CurrentCount = 0 };
        _quotaRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quota);
        _assignmentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<QuotaAssignment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<QuotaAssignment>());

        var command = new UnassignCandidacyCommand(QuotaId: 10, CandidacyId: 100);

        var act = () => _sut.UnassignCandidacyAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetFulfillmentStatusAsync

    [Fact]
    public async Task GetFulfillmentStatusAsync_ReturnsCorrectFulfillmentPercentages()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _quotaRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Quota, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Quota>
            {
                new() { Id = 10, OrgUnitId = 1, CategoryName = "נשים", TargetCount = 4, CurrentCount = 2, IsActive = true },
                new() { Id = 11, OrgUnitId = 1, CategoryName = "מגזר ערבי", TargetCount = 5, CurrentCount = 5, IsActive = true }
            });

        var result = await _sut.GetFulfillmentStatusAsync(1);

        result.Should().NotBeNull();
        result.OrgUnitId.Should().Be(1);
        var quotas = result.Quotas.ToList();
        quotas.Should().HaveCount(2);
        quotas[0].FulfillmentPercentage.Should().Be(50.0);
        quotas[1].FulfillmentPercentage.Should().Be(100.0);
    }

    [Fact]
    public async Task GetFulfillmentStatusAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var act = () => _sut.GetFulfillmentStatusAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetFulfillmentStatusAsync_NoQuotas_ReturnsEmptyList()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _quotaRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Quota, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Quota>());

        var result = await _sut.GetFulfillmentStatusAsync(1);

        result.Quotas.Should().BeEmpty();
    }

    #endregion
}
