using System.Linq.Expressions;
using CandidacyManagement.Application.Dashboard;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Dashboard;

public class DashboardServiceTests
{
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<CandidacyStatusHistory>> _historyRepoMock;
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();

        _sut = new DashboardService(
            _candidacyRepoMock.Object,
            _orgUnitRepoMock.Object,
            _statusRepoMock.Object,
            _historyRepoMock.Object);
    }

    // --- GetDashboardDataAsync ---

    [Fact]
    public async Task GetDashboardData_ReturnsActiveCandidaciesCount()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה", CandidacyStatusCategory.Submitted) });
        SetupActiveCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 1, 100, 10),
            MakeCandidacy(3, 1, 100, 10)
        });
        SetupHistories(Array.Empty<CandidacyStatusHistory>());

        var result = await _sut.GetDashboardDataAsync(1);

        result.ActiveCandidacies.Should().Be(3);
        result.OrgUnitId.Should().Be(1);
        result.OrgUnitName.Should().Be("עוזמ\"ת");
    }

    [Fact]
    public async Task GetDashboardData_ReturnsBreakdownByScreeningStage()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[]
        {
            (10, "submitted", "הוגשה", CandidacyStatusCategory.Submitted),
            (20, "exam", "מבחן", CandidacyStatusCategory.Exam),
            (30, "interview", "ראיון", CandidacyStatusCategory.Interview)
        });
        SetupActiveCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 1, 100, 20),
            MakeCandidacy(3, 1, 100, 20),
            MakeCandidacy(4, 1, 100, 30)
        });
        SetupHistories(Array.Empty<CandidacyStatusHistory>());

        var result = await _sut.GetDashboardDataAsync(1);

        result.ByScreeningStage.Should().HaveCount(3);
        result.ByScreeningStage.First(s => s.StageCategory == "Submitted").Count.Should().Be(1);
        result.ByScreeningStage.First(s => s.StageCategory == "Exam").Count.Should().Be(2);
        result.ByScreeningStage.First(s => s.StageCategory == "Interview").Count.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardData_IdentifiesCandidaciesRequiringAttention()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה", CandidacyStatusCategory.Submitted) });

        var staleCandidacy = MakeCandidacy(1, 1, 100, 10, DateTime.UtcNow.AddDays(-30));
        var recentCandidacy = MakeCandidacy(2, 1, 100, 10, DateTime.UtcNow.AddDays(-1));
        SetupActiveCandidacies(new[] { staleCandidacy, recentCandidacy });

        // Stale candidacy has old history, recent has new history
        SetupHistories(new[]
        {
            new CandidacyStatusHistory
            {
                Id = 1, CandidacyId = 1, ToStatusId = 10,
                ChangedAt = DateTime.UtcNow.AddDays(-20), ChangedByUserId = 1
            },
            new CandidacyStatusHistory
            {
                Id = 2, CandidacyId = 2, ToStatusId = 10,
                ChangedAt = DateTime.UtcNow.AddDays(-1), ChangedByUserId = 1
            }
        });

        var result = await _sut.GetDashboardDataAsync(1);

        result.CandidaciesRequiringAttention.Should().Be(1);
        result.AttentionItems.Should().ContainSingle();
        result.AttentionItems.First().CandidacyId.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardData_CandidacyWithNoHistory_UsesCreatedAtForAttention()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה", CandidacyStatusCategory.Submitted) });

        var oldCandidacy = MakeCandidacy(1, 1, 100, 10, DateTime.UtcNow.AddDays(-30));
        SetupActiveCandidacies(new[] { oldCandidacy });
        SetupHistories(Array.Empty<CandidacyStatusHistory>());

        var result = await _sut.GetDashboardDataAsync(1);

        result.CandidaciesRequiringAttention.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardData_NoCandidacies_ReturnsEmptyDashboard()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, Array.Empty<(int, string, string, CandidacyStatusCategory)>());
        SetupActiveCandidacies(Array.Empty<Candidacy>());
        SetupHistories(Array.Empty<CandidacyStatusHistory>());

        var result = await _sut.GetDashboardDataAsync(1);

        result.ActiveCandidacies.Should().Be(0);
        result.ByScreeningStage.Should().BeEmpty();
        result.CandidaciesRequiringAttention.Should().Be(0);
        result.AttentionItems.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardData_NonExistentOrgUnit_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var act = () => _sut.GetDashboardDataAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- GetDashboardByOrgUnitAsync ---

    [Fact]
    public async Task GetDashboardByOrgUnit_ReturnsSummaryForOrgUnit()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[]
        {
            (10, "submitted", "הוגשה", CandidacyStatusCategory.Submitted),
            (20, "exam", "מבחן", CandidacyStatusCategory.Exam)
        });
        SetupActiveCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 1, 100, 20)
        });
        SetupHistories(Array.Empty<CandidacyStatusHistory>());

        var result = await _sut.GetDashboardByOrgUnitAsync(1);

        result.OrgUnitId.Should().Be(1);
        result.OrgUnitName.Should().Be("עוזמ\"ת");
        result.ActiveCandidacies.Should().Be(2);
        result.ByScreeningStage.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDashboardByOrgUnit_NonExistentOrgUnit_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var act = () => _sut.GetDashboardByOrgUnitAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetDashboardByOrgUnit_IncludesAttentionCount()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה", CandidacyStatusCategory.Submitted) });

        var staleCandidacy = MakeCandidacy(1, 1, 100, 10, DateTime.UtcNow.AddDays(-30));
        var recentCandidacy = MakeCandidacy(2, 1, 100, 10, DateTime.UtcNow.AddDays(-1));
        SetupActiveCandidacies(new[] { staleCandidacy, recentCandidacy });

        SetupHistories(new[]
        {
            new CandidacyStatusHistory
            {
                Id = 1, CandidacyId = 1, ToStatusId = 10,
                ChangedAt = DateTime.UtcNow.AddDays(-20), ChangedByUserId = 1
            },
            new CandidacyStatusHistory
            {
                Id = 2, CandidacyId = 2, ToStatusId = 10,
                ChangedAt = DateTime.UtcNow.AddDays(-1), ChangedByUserId = 1
            }
        });

        var result = await _sut.GetDashboardByOrgUnitAsync(1);

        result.CandidaciesRequiringAttention.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardData_StageDisplayNames_AreHebrew()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[]
        {
            (10, "submitted", "הוגשה", CandidacyStatusCategory.Submitted),
            (20, "accepted", "התקבל", CandidacyStatusCategory.Accepted)
        });
        SetupActiveCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 1, 100, 20)
        });
        SetupHistories(Array.Empty<CandidacyStatusHistory>());

        var result = await _sut.GetDashboardDataAsync(1);

        result.ByScreeningStage.First(s => s.StageCategory == "Submitted").StageDisplayName.Should().Be("הוגשה");
        result.ByScreeningStage.First(s => s.StageCategory == "Accepted").StageDisplayName.Should().Be("התקבל");
    }

    // --- Helpers ---

    private void SetupOrgUnit(int id, string name)
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = id, Name = name });
    }

    private void SetupStatuses(int orgUnitId,
        (int id, string code, string displayName, CandidacyStatusCategory category)[] statuses)
    {
        var entities = statuses.Select(s => new StatusDefinition
        {
            Id = s.id, OrgUnitId = orgUnitId, Code = s.code,
            DisplayName = s.displayName, Category = s.category
        }).ToList();

        _statusRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
    }

    private void SetupActiveCandidacies(IEnumerable<Candidacy> candidacies)
    {
        var list = candidacies.ToList();
        _candidacyRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
    }

    private void SetupHistories(IEnumerable<CandidacyStatusHistory> histories)
    {
        var list = histories.ToList();
        _historyRepoMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<CandidacyStatusHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
    }

    private static Candidacy MakeCandidacy(
        int id, int orgUnitId, int callId, int statusId, DateTime? createdAt = null) =>
        new()
        {
            Id = id,
            OrgUnitId = orgUnitId,
            CallForCandidatesId = callId,
            CurrentStatusId = statusId,
            IsActive = true,
            ContactId = id * 10,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
}
