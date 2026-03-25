using System.Text;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Reports;

public class ReportService : IReportService
{
    private readonly IRepository<Candidacy> _candidacyRepo;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepo;
    private readonly IRepository<CallForCandidates> _callRepo;
    private readonly IRepository<StatusDefinition> _statusRepo;
    private readonly IRepository<CustomReportDefinition> _customReportRepo;

    public ReportService(
        IRepository<Candidacy> candidacyRepo,
        IRepository<OrganizationalUnit> orgUnitRepo,
        IRepository<CallForCandidates> callRepo,
        IRepository<StatusDefinition> statusRepo,
        IRepository<CustomReportDefinition> customReportRepo)
    {
        _candidacyRepo = candidacyRepo;
        _orgUnitRepo = orgUnitRepo;
        _callRepo = callRepo;
        _statusRepo = statusRepo;
        _customReportRepo = customReportRepo;
    }

    public async Task<StatusReportDto> GetCandidacyStatusReportAsync(
        StatusReportParams parameters, CancellationToken cancellationToken = default)
    {
        var orgUnit = await _orgUnitRepo.GetByIdAsync(parameters.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException("OrganizationalUnit", parameters.OrgUnitId);

        var candidacies = await _candidacyRepo.FindAsync(
            c => c.OrgUnitId == parameters.OrgUnitId, cancellationToken);

        var candidacyList = candidacies.ToList();

        // Apply filters
        if (parameters.CallForCandidatesId.HasValue)
            candidacyList = candidacyList
                .Where(c => c.CallForCandidatesId == parameters.CallForCandidatesId.Value).ToList();

        if (parameters.FromDate.HasValue)
            candidacyList = candidacyList
                .Where(c => c.CreatedAt >= parameters.FromDate.Value).ToList();

        if (parameters.ToDate.HasValue)
            candidacyList = candidacyList
                .Where(c => c.CreatedAt <= parameters.ToDate.Value).ToList();

        var statuses = await _statusRepo.FindAsync(
            s => s.OrgUnitId == parameters.OrgUnitId, cancellationToken);
        var statusLookup = statuses.ToDictionary(s => s.Id, s => s);

        // Filter by status code if specified
        if (!string.IsNullOrEmpty(parameters.StatusCode))
        {
            var matchingStatusIds = statusLookup.Values
                .Where(s => s.Code == parameters.StatusCode)
                .Select(s => s.Id)
                .ToHashSet();
            candidacyList = candidacyList
                .Where(c => c.CurrentStatusId.HasValue && matchingStatusIds.Contains(c.CurrentStatusId.Value))
                .ToList();
        }

        // Build status breakdown
        var byStatus = candidacyList
            .Where(c => c.CurrentStatusId.HasValue)
            .GroupBy(c => c.CurrentStatusId!.Value)
            .Select(g =>
            {
                var status = statusLookup.GetValueOrDefault(g.Key);
                return new StatusBreakdownDto(
                    StatusCode: status?.Code ?? "Unknown",
                    StatusDisplayName: status?.DisplayName ?? "Unknown",
                    Count: g.Count());
            })
            .OrderBy(s => s.StatusCode)
            .ToList();

        // Build call breakdown
        var calls = await _callRepo.FindAsync(
            c => c.OrgUnitId == parameters.OrgUnitId, cancellationToken);
        var callLookup = calls.ToDictionary(c => c.Id, c => c);

        var byCall = candidacyList
            .GroupBy(c => c.CallForCandidatesId)
            .Select(g =>
            {
                var call = callLookup.GetValueOrDefault(g.Key);
                var callStatusBreakdown = g
                    .Where(c => c.CurrentStatusId.HasValue)
                    .GroupBy(c => c.CurrentStatusId!.Value)
                    .Select(sg =>
                    {
                        var status = statusLookup.GetValueOrDefault(sg.Key);
                        return new StatusBreakdownDto(
                            StatusCode: status?.Code ?? "Unknown",
                            StatusDisplayName: status?.DisplayName ?? "Unknown",
                            Count: sg.Count());
                    })
                    .OrderBy(s => s.StatusCode)
                    .ToList();

                return new CallBreakdownDto(
                    CallForCandidatesId: g.Key,
                    CallTitle: call?.Title ?? "Unknown",
                    TotalCandidacies: g.Count(),
                    ByStatus: callStatusBreakdown);
            })
            .ToList();

        return new StatusReportDto(
            OrgUnitId: parameters.OrgUnitId,
            OrgUnitName: orgUnit.Name,
            TotalCandidacies: candidacyList.Count,
            ByStatus: byStatus,
            ByCall: byCall);
    }

    public async Task<CrossUnitReportDto> GetCrossUnitReportAsync(
        CrossUnitReportParams parameters, CancellationToken cancellationToken = default)
    {
        var allOrgUnits = await _orgUnitRepo.GetAllAsync(cancellationToken);
        var orgUnitList = allOrgUnits.ToList();

        if (parameters.OrgUnitIds != null && parameters.OrgUnitIds.Any())
        {
            var ids = parameters.OrgUnitIds.ToHashSet();
            orgUnitList = orgUnitList.Where(u => ids.Contains(u.Id)).ToList();
        }

        var allCandidacies = await _candidacyRepo.GetAllAsync(cancellationToken);
        var candidacyList = allCandidacies.ToList();

        if (parameters.FromDate.HasValue)
            candidacyList = candidacyList.Where(c => c.CreatedAt >= parameters.FromDate.Value).ToList();
        if (parameters.ToDate.HasValue)
            candidacyList = candidacyList.Where(c => c.CreatedAt <= parameters.ToDate.Value).ToList();

        var orgUnitLookup = orgUnitList.ToDictionary(u => u.Id, u => u);
        var relevantOrgUnitIds = orgUnitList.Select(u => u.Id).ToHashSet();
        candidacyList = candidacyList.Where(c => relevantOrgUnitIds.Contains(c.OrgUnitId)).ToList();

        var allStatuses = await _statusRepo.GetAllAsync(cancellationToken);
        var statusLookup = allStatuses.ToDictionary(s => s.Id, s => s);

        var units = candidacyList
            .GroupBy(c => c.OrgUnitId)
            .Select(g =>
            {
                var orgUnit = orgUnitLookup.GetValueOrDefault(g.Key);
                var byStatus = g
                    .Where(c => c.CurrentStatusId.HasValue)
                    .GroupBy(c => c.CurrentStatusId!.Value)
                    .Select(sg =>
                    {
                        var status = statusLookup.GetValueOrDefault(sg.Key);
                        return new StatusBreakdownDto(
                            StatusCode: status?.Code ?? "Unknown",
                            StatusDisplayName: status?.DisplayName ?? "Unknown",
                            Count: sg.Count());
                    })
                    .OrderBy(s => s.StatusCode)
                    .ToList();

                return new UnitSummaryDto(
                    OrgUnitId: g.Key,
                    OrgUnitName: orgUnit?.Name ?? "Unknown",
                    TotalCandidacies: g.Count(),
                    ActiveCandidacies: g.Count(c => c.IsActive),
                    ByStatus: byStatus);
            })
            .ToList();

        return new CrossUnitReportDto(
            GeneratedAt: DateTime.UtcNow,
            TotalCandidacies: candidacyList.Count,
            Units: units);
    }

    public async Task<byte[]> ExportToExcelAsync(
        ExportReportParams parameters, CancellationToken cancellationToken = default)
    {
        // Generate CSV-based Excel export (no external library dependency)
        var sb = new StringBuilder();

        if (parameters.ReportType == "status" && parameters.OrgUnitId.HasValue)
        {
            var report = await GetCandidacyStatusReportAsync(
                new StatusReportParams(
                    parameters.OrgUnitId.Value,
                    parameters.CallForCandidatesId,
                    parameters.StatusCode,
                    parameters.FromDate,
                    parameters.ToDate),
                cancellationToken);

            sb.AppendLine("StatusCode,StatusDisplayName,Count");
            foreach (var row in report.ByStatus)
                sb.AppendLine($"{EscapeCsv(row.StatusCode)},{EscapeCsv(row.StatusDisplayName)},{row.Count}");

            sb.AppendLine();
            sb.AppendLine("CallForCandidatesId,CallTitle,TotalCandidacies");
            foreach (var row in report.ByCall)
                sb.AppendLine($"{row.CallForCandidatesId},{EscapeCsv(row.CallTitle)},{row.TotalCandidacies}");
        }
        else if (parameters.ReportType == "cross-unit")
        {
            var report = await GetCrossUnitReportAsync(
                new CrossUnitReportParams(parameters.OrgUnitIds, parameters.FromDate, parameters.ToDate),
                cancellationToken);

            sb.AppendLine("OrgUnitId,OrgUnitName,TotalCandidacies,ActiveCandidacies");
            foreach (var unit in report.Units)
                sb.AppendLine($"{unit.OrgUnitId},{EscapeCsv(unit.OrgUnitName)},{unit.TotalCandidacies},{unit.ActiveCandidacies}");
        }
        else
        {
            throw new ValidationException("ReportType", "Unsupported report type. Use 'status' or 'cross-unit'.");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<ReportResult> GetCustomReportAsync(
        CustomReportParams parameters, CancellationToken cancellationToken = default)
    {
        var definition = await _customReportRepo.GetByIdAsync(
            parameters.CustomReportDefinitionId, cancellationToken)
            ?? throw new NotFoundException("CustomReportDefinition", parameters.CustomReportDefinitionId);

        if (definition.OrgUnitId != parameters.OrgUnitId)
            throw new ValidationException("OrgUnitId", "Custom report definition does not belong to the specified organizational unit.");

        if (!definition.IsActive)
            throw new ValidationException("CustomReportDefinition", "The custom report definition is not active.");

        var candidacies = await _candidacyRepo.FindAsync(
            c => c.OrgUnitId == parameters.OrgUnitId, cancellationToken);
        var candidacyList = candidacies.ToList();

        if (parameters.FromDate.HasValue)
            candidacyList = candidacyList.Where(c => c.CreatedAt >= parameters.FromDate.Value).ToList();
        if (parameters.ToDate.HasValue)
            candidacyList = candidacyList.Where(c => c.CreatedAt <= parameters.ToDate.Value).ToList();

        var statuses = await _statusRepo.FindAsync(
            s => s.OrgUnitId == parameters.OrgUnitId, cancellationToken);
        var statusLookup = statuses.ToDictionary(s => s.Id, s => s);

        var rows = candidacyList.Select(c =>
        {
            var status = c.CurrentStatusId.HasValue
                ? statusLookup.GetValueOrDefault(c.CurrentStatusId.Value)
                : null;

            return new ReportRow(new Dictionary<string, object?>
            {
                ["CandidacyId"] = c.Id,
                ["ContactId"] = c.ContactId,
                ["CallForCandidatesId"] = c.CallForCandidatesId,
                ["StatusCode"] = status?.Code,
                ["StatusDisplayName"] = status?.DisplayName,
                ["IsActive"] = c.IsActive,
                ["CreatedAt"] = c.CreatedAt,
                ["SubmittedAt"] = c.SubmittedAt
            });
        }).ToList();

        var aggregations = candidacyList
            .Where(c => c.CurrentStatusId.HasValue)
            .GroupBy(c => c.CurrentStatusId!.Value)
            .Select(g =>
            {
                var status = statusLookup.GetValueOrDefault(g.Key);
                return new ReportAggregation(
                    GroupKey: "Status",
                    GroupValue: status?.DisplayName ?? "Unknown",
                    Count: g.Count());
            })
            .ToList();

        return new ReportResult(
            ReportName: definition.Name,
            GeneratedAt: DateTime.UtcNow,
            TotalRecords: candidacyList.Count,
            Rows: rows,
            Aggregations: aggregations);
    }

    // --- Custom Report Definition CRUD ---

    public async Task<CustomReportDefinitionDto> CreateCustomReportDefinitionAsync(
        CreateCustomReportDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "Report definition name is required.");

        await _orgUnitRepo.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException("OrganizationalUnit", command.OrgUnitId);

        var entity = new CustomReportDefinition
        {
            OrgUnitId = command.OrgUnitId,
            Name = command.Name,
            Description = command.Description,
            ColumnsJson = command.ColumnsJson,
            FiltersJson = command.FiltersJson,
            GroupByJson = command.GroupByJson,
            SortOrderJson = command.SortOrderJson,
            IsActive = true
        };

        var created = await _customReportRepo.AddAsync(entity, cancellationToken);
        return ToDto(created);
    }

    public async Task<CustomReportDefinitionDto> UpdateCustomReportDefinitionAsync(
        UpdateCustomReportDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _customReportRepo.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("CustomReportDefinition", command.Id);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "Report definition name is required.");

        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.ColumnsJson = command.ColumnsJson;
        entity.FiltersJson = command.FiltersJson;
        entity.GroupByJson = command.GroupByJson;
        entity.SortOrderJson = command.SortOrderJson;
        entity.IsActive = command.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _customReportRepo.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<IEnumerable<CustomReportDefinitionDto>> GetCustomReportDefinitionsAsync(
        int orgUnitId, CancellationToken cancellationToken = default)
    {
        var definitions = await _customReportRepo.FindAsync(
            d => d.OrgUnitId == orgUnitId, cancellationToken);
        return definitions.Select(ToDto);
    }

    public async Task DeleteCustomReportDefinitionAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var entity = await _customReportRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("CustomReportDefinition", id);
        await _customReportRepo.DeleteAsync(entity, cancellationToken);
    }

    // --- Helpers ---

    private static CustomReportDefinitionDto ToDto(CustomReportDefinition entity) =>
        new(
            Id: entity.Id,
            OrgUnitId: entity.OrgUnitId,
            Name: entity.Name,
            Description: entity.Description,
            ColumnsJson: entity.ColumnsJson,
            FiltersJson: entity.FiltersJson,
            GroupByJson: entity.GroupByJson,
            SortOrderJson: entity.SortOrderJson,
            IsActive: entity.IsActive);

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
