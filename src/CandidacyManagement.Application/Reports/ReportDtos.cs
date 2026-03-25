namespace CandidacyManagement.Application.Reports;

// --- Parameters ---

public record StatusReportParams(
    int OrgUnitId,
    int? CallForCandidatesId = null,
    string? StatusCode = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);

public record CrossUnitReportParams(
    IEnumerable<int>? OrgUnitIds = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);

public record ExportReportParams(
    string ReportType,
    int? OrgUnitId = null,
    int? CallForCandidatesId = null,
    string? StatusCode = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    IEnumerable<int>? OrgUnitIds = null);

public record CustomReportParams(
    int OrgUnitId,
    int CustomReportDefinitionId,
    DateTime? FromDate = null,
    DateTime? ToDate = null);

// --- Results ---

public record ReportResult(
    string ReportName,
    DateTime GeneratedAt,
    int TotalRecords,
    IEnumerable<ReportRow> Rows,
    IEnumerable<ReportAggregation> Aggregations);

public record ReportRow(
    Dictionary<string, object?> Values);

public record ReportAggregation(
    string GroupKey,
    string GroupValue,
    int Count);

// --- DTOs ---

public record StatusReportDto(
    int OrgUnitId,
    string OrgUnitName,
    int TotalCandidacies,
    IEnumerable<StatusBreakdownDto> ByStatus,
    IEnumerable<CallBreakdownDto> ByCall);

public record StatusBreakdownDto(
    string StatusCode,
    string StatusDisplayName,
    int Count);

public record CallBreakdownDto(
    int CallForCandidatesId,
    string CallTitle,
    int TotalCandidacies,
    IEnumerable<StatusBreakdownDto> ByStatus);

public record CrossUnitReportDto(
    DateTime GeneratedAt,
    int TotalCandidacies,
    IEnumerable<UnitSummaryDto> Units);

public record UnitSummaryDto(
    int OrgUnitId,
    string OrgUnitName,
    int TotalCandidacies,
    int ActiveCandidacies,
    IEnumerable<StatusBreakdownDto> ByStatus);

public record CustomReportDefinitionDto(
    int Id,
    int OrgUnitId,
    string Name,
    string? Description,
    string ColumnsJson,
    string FiltersJson,
    string? GroupByJson,
    string? SortOrderJson,
    bool IsActive);

public record CreateCustomReportDefinitionCommand(
    int OrgUnitId,
    string Name,
    string? Description,
    string ColumnsJson,
    string FiltersJson,
    string? GroupByJson,
    string? SortOrderJson);

public record UpdateCustomReportDefinitionCommand(
    int Id,
    string Name,
    string? Description,
    string ColumnsJson,
    string FiltersJson,
    string? GroupByJson,
    string? SortOrderJson,
    bool IsActive);
