using System.Linq.Expressions;
using CandidacyManagement.Application.Reports;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Reports;

public class ReportServiceTests
{
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<CallForCandidates>> _callRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<CustomReportDefinition>> _customReportRepoMock;
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _callRepoMock = new Mock<IRepository<CallForCandidates>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _customReportRepoMock = new Mock<IRepository<CustomReportDefinition>>();

        _sut = new ReportService(
            _candidacyRepoMock.Object,
            _orgUnitRepoMock.Object,
            _callRepoMock.Object,
            _statusRepoMock.Object,
            _customReportRepoMock.Object);
    }

    // --- GetCandidacyStatusReportAsync ---

    [Fact]
    public async Task GetCandidacyStatusReport_ReturnsBreakdownByStatus()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה"), (20, "accepted", "התקבל") });
        SetupCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 1, 100, 10),
            MakeCandidacy(3, 1, 100, 20)
        });
        SetupCalls(1, new[] { (100, "קול קורא 2024") });

        var result = await _sut.GetCandidacyStatusReportAsync(new StatusReportParams(OrgUnitId: 1));

        result.OrgUnitId.Should().Be(1);
        result.TotalCandidacies.Should().Be(3);
        result.ByStatus.Should().HaveCount(2);
        result.ByStatus.First(s => s.StatusCode == "submitted").Count.Should().Be(2);
        result.ByStatus.First(s => s.StatusCode == "accepted").Count.Should().Be(1);
    }

    [Fact]
    public async Task GetCandidacyStatusReport_FilterByCall_ReturnsOnlyMatchingCandidacies()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה") });
        SetupCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 1, 200, 10)
        });
        SetupCalls(1, new[] { (100, "קול קורא א"), (200, "קול קורא ב") });

        var result = await _sut.GetCandidacyStatusReportAsync(
            new StatusReportParams(OrgUnitId: 1, CallForCandidatesId: 100));

        result.TotalCandidacies.Should().Be(1);
    }

    [Fact]
    public async Task GetCandidacyStatusReport_FilterByDateRange_ReturnsOnlyMatchingCandidacies()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה") });
        SetupCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10, new DateTime(2024, 1, 15)),
            MakeCandidacy(2, 1, 100, 10, new DateTime(2024, 6, 15))
        });
        SetupCalls(1, new[] { (100, "קול קורא") });

        var result = await _sut.GetCandidacyStatusReportAsync(
            new StatusReportParams(OrgUnitId: 1, FromDate: new DateTime(2024, 6, 1)));

        result.TotalCandidacies.Should().Be(1);
    }

    [Fact]
    public async Task GetCandidacyStatusReport_FilterByStatusCode_ReturnsOnlyMatchingStatus()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה"), (20, "accepted", "התקבל") });
        SetupCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 1, 100, 20)
        });
        SetupCalls(1, new[] { (100, "קול קורא") });

        var result = await _sut.GetCandidacyStatusReportAsync(
            new StatusReportParams(OrgUnitId: 1, StatusCode: "accepted"));

        result.TotalCandidacies.Should().Be(1);
        result.ByStatus.Should().ContainSingle(s => s.StatusCode == "accepted");
    }

    [Fact]
    public async Task GetCandidacyStatusReport_NonExistentOrgUnit_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var act = () => _sut.GetCandidacyStatusReportAsync(new StatusReportParams(OrgUnitId: 999));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCandidacyStatusReport_NoCandidacies_ReturnsEmptyReport()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, Array.Empty<(int, string, string)>());
        SetupCandidacies(Array.Empty<Candidacy>());
        SetupCalls(1, Array.Empty<(int, string)>());

        var result = await _sut.GetCandidacyStatusReportAsync(new StatusReportParams(OrgUnitId: 1));

        result.TotalCandidacies.Should().Be(0);
        result.ByStatus.Should().BeEmpty();
        result.ByCall.Should().BeEmpty();
    }

    // --- GetCrossUnitReportAsync ---

    [Fact]
    public async Task GetCrossUnitReport_ReturnsAggregatedDataAcrossUnits()
    {
        SetupAllOrgUnits(new[] { (1, "עוזמ\"ת"), (2, "נציגי ציבור") });
        SetupAllStatuses(new[]
        {
            (10, 1, "submitted", "הוגשה"),
            (20, 2, "submitted", "הוגשה")
        });
        SetupAllCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 1, 100, 10),
            MakeCandidacy(3, 2, 200, 20)
        });

        var result = await _sut.GetCrossUnitReportAsync(new CrossUnitReportParams());

        result.TotalCandidacies.Should().Be(3);
        result.Units.Should().HaveCount(2);
        result.Units.First(u => u.OrgUnitId == 1).TotalCandidacies.Should().Be(2);
        result.Units.First(u => u.OrgUnitId == 2).TotalCandidacies.Should().Be(1);
    }

    [Fact]
    public async Task GetCrossUnitReport_FilterByOrgUnitIds_ReturnsOnlySpecifiedUnits()
    {
        SetupAllOrgUnits(new[] { (1, "עוזמ\"ת"), (2, "נציגי ציבור") });
        SetupAllStatuses(new[]
        {
            (10, 1, "submitted", "הוגשה"),
            (20, 2, "submitted", "הוגשה")
        });
        SetupAllCandidacies(new[]
        {
            MakeCandidacy(1, 1, 100, 10),
            MakeCandidacy(2, 2, 200, 20)
        });

        var result = await _sut.GetCrossUnitReportAsync(
            new CrossUnitReportParams(OrgUnitIds: new[] { 1 }));

        result.TotalCandidacies.Should().Be(1);
        result.Units.Should().ContainSingle(u => u.OrgUnitId == 1);
    }

    // --- ExportToExcelAsync ---

    [Fact]
    public async Task ExportToExcel_StatusReport_ReturnsCsvBytes()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה") });
        SetupCandidacies(new[] { MakeCandidacy(1, 1, 100, 10) });
        SetupCalls(1, new[] { (100, "קול קורא") });

        var bytes = await _sut.ExportToExcelAsync(
            new ExportReportParams(ReportType: "status", OrgUnitId: 1));

        bytes.Should().NotBeEmpty();
        var content = System.Text.Encoding.UTF8.GetString(bytes);
        content.Should().Contain("StatusCode");
        content.Should().Contain("submitted");
    }

    [Fact]
    public async Task ExportToExcel_CrossUnitReport_ReturnsCsvBytes()
    {
        SetupAllOrgUnits(new[] { (1, "עוזמ\"ת") });
        SetupAllStatuses(new[] { (10, 1, "submitted", "הוגשה") });
        SetupAllCandidacies(new[] { MakeCandidacy(1, 1, 100, 10) });

        var bytes = await _sut.ExportToExcelAsync(
            new ExportReportParams(ReportType: "cross-unit"));

        bytes.Should().NotBeEmpty();
        var content = System.Text.Encoding.UTF8.GetString(bytes);
        content.Should().Contain("OrgUnitId");
    }

    [Fact]
    public async Task ExportToExcel_UnsupportedReportType_ThrowsValidationException()
    {
        var act = () => _sut.ExportToExcelAsync(
            new ExportReportParams(ReportType: "invalid"));

        await act.Should().ThrowAsync<ValidationException>();
    }

    // --- GetCustomReportAsync ---

    [Fact]
    public async Task GetCustomReport_ValidDefinition_ReturnsReportResult()
    {
        var definition = new CustomReportDefinition
        {
            Id = 1, OrgUnitId = 1, Name = "דוח מותאם", IsActive = true,
            ColumnsJson = "[]", FiltersJson = "{}"
        };
        _customReportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);
        SetupStatuses(1, new[] { (10, "submitted", "הוגשה") });
        SetupCandidacies(new[] { MakeCandidacy(1, 1, 100, 10) });

        var result = await _sut.GetCustomReportAsync(
            new CustomReportParams(OrgUnitId: 1, CustomReportDefinitionId: 1));

        result.ReportName.Should().Be("דוח מותאם");
        result.TotalRecords.Should().Be(1);
        result.Rows.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCustomReport_WrongOrgUnit_ThrowsValidationException()
    {
        var definition = new CustomReportDefinition
        {
            Id = 1, OrgUnitId = 2, Name = "דוח", IsActive = true,
            ColumnsJson = "[]", FiltersJson = "{}"
        };
        _customReportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        var act = () => _sut.GetCustomReportAsync(
            new CustomReportParams(OrgUnitId: 1, CustomReportDefinitionId: 1));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetCustomReport_InactiveDefinition_ThrowsValidationException()
    {
        var definition = new CustomReportDefinition
        {
            Id = 1, OrgUnitId = 1, Name = "דוח", IsActive = false,
            ColumnsJson = "[]", FiltersJson = "{}"
        };
        _customReportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        var act = () => _sut.GetCustomReportAsync(
            new CustomReportParams(OrgUnitId: 1, CustomReportDefinitionId: 1));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetCustomReport_NonExistentDefinition_ThrowsNotFoundException()
    {
        _customReportRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomReportDefinition?)null);

        var act = () => _sut.GetCustomReportAsync(
            new CustomReportParams(OrgUnitId: 1, CustomReportDefinitionId: 999));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- Custom Report Definition CRUD ---

    [Fact]
    public async Task CreateCustomReportDefinition_ValidCommand_ReturnsDto()
    {
        SetupOrgUnit(1, "עוזמ\"ת");
        _customReportRepoMock.Setup(r => r.AddAsync(It.IsAny<CustomReportDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomReportDefinition e, CancellationToken _) => { e.Id = 1; return e; });

        var command = new CreateCustomReportDefinitionCommand(
            OrgUnitId: 1, Name: "דוח חדש", Description: "תיאור",
            ColumnsJson: "[\"col1\"]", FiltersJson: "{}", GroupByJson: null, SortOrderJson: null);

        var result = await _sut.CreateCustomReportDefinitionAsync(command);

        result.Name.Should().Be("דוח חדש");
        result.OrgUnitId.Should().Be(1);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomReportDefinition_EmptyName_ThrowsValidationException()
    {
        var command = new CreateCustomReportDefinitionCommand(
            OrgUnitId: 1, Name: "", Description: null,
            ColumnsJson: "[]", FiltersJson: "{}", GroupByJson: null, SortOrderJson: null);

        var act = () => _sut.CreateCustomReportDefinitionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateCustomReportDefinition_ValidCommand_ReturnsUpdatedDto()
    {
        var existing = new CustomReportDefinition
        {
            Id = 1, OrgUnitId = 1, Name = "ישן", IsActive = true,
            ColumnsJson = "[]", FiltersJson = "{}"
        };
        _customReportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var command = new UpdateCustomReportDefinitionCommand(
            Id: 1, Name: "חדש", Description: "עדכון",
            ColumnsJson: "[\"col1\"]", FiltersJson: "{}", GroupByJson: null, SortOrderJson: null, IsActive: true);

        var result = await _sut.UpdateCustomReportDefinitionAsync(command);

        result.Name.Should().Be("חדש");
        result.Description.Should().Be("עדכון");
    }

    [Fact]
    public async Task DeleteCustomReportDefinition_ExistingId_DeletesSuccessfully()
    {
        var existing = new CustomReportDefinition { Id = 1, OrgUnitId = 1, Name = "דוח" };
        _customReportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.DeleteCustomReportDefinitionAsync(1);

        _customReportRepoMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomReportDefinition_NonExistentId_ThrowsNotFoundException()
    {
        _customReportRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomReportDefinition?)null);

        var act = () => _sut.DeleteCustomReportDefinitionAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCustomReportDefinitions_ReturnsDefinitionsForOrgUnit()
    {
        var definitions = new List<CustomReportDefinition>
        {
            new() { Id = 1, OrgUnitId = 1, Name = "דוח א", IsActive = true, ColumnsJson = "[]", FiltersJson = "{}" },
            new() { Id = 2, OrgUnitId = 1, Name = "דוח ב", IsActive = true, ColumnsJson = "[]", FiltersJson = "{}" }
        };
        _customReportRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CustomReportDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(definitions);

        var result = await _sut.GetCustomReportDefinitionsAsync(1);

        result.Should().HaveCount(2);
    }

    // --- Helpers ---

    private void SetupOrgUnit(int id, string name)
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = id, Name = name });
    }

    private void SetupStatuses(int orgUnitId, (int id, string code, string displayName)[] statuses)
    {
        var entities = statuses.Select(s => new StatusDefinition
        {
            Id = s.id, OrgUnitId = orgUnitId, Code = s.code, DisplayName = s.displayName
        }).ToList();

        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
    }

    private void SetupCandidacies(IEnumerable<Candidacy> candidacies)
    {
        var list = candidacies.ToList();
        _candidacyRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
    }

    private void SetupCalls(int orgUnitId, (int id, string title)[] calls)
    {
        var entities = calls.Select(c => new CallForCandidates
        {
            Id = c.id, OrgUnitId = orgUnitId, Title = c.title
        }).ToList();

        _callRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CallForCandidates, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
    }

    private void SetupAllOrgUnits((int id, string name)[] units)
    {
        var entities = units.Select(u => new OrganizationalUnit { Id = u.id, Name = u.name }).ToList();
        _orgUnitRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
    }

    private void SetupAllStatuses((int id, int orgUnitId, string code, string displayName)[] statuses)
    {
        var entities = statuses.Select(s => new StatusDefinition
        {
            Id = s.id, OrgUnitId = s.orgUnitId, Code = s.code, DisplayName = s.displayName
        }).ToList();

        _statusRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
    }

    private void SetupAllCandidacies(IEnumerable<Candidacy> candidacies)
    {
        var list = candidacies.ToList();
        _candidacyRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
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
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
}
