namespace CandidacyManagement.Application.Reports;

public interface IReportService
{
    /// <summary>
    /// דוח סטטוס מועמדויות ברמת יחידה ארגונית עם פילוח לפי סטטוס, קול קורא ותקופה
    /// </summary>
    Task<StatusReportDto> GetCandidacyStatusReportAsync(
        StatusReportParams parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// דוח מאוחד חוצה יחידות ארגוניות למשתמשים מורשים
    /// </summary>
    Task<CrossUnitReportDto> GetCrossUnitReportAsync(
        CrossUnitReportParams parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// ייצוא דוח לפורמט Excel - מחזיר מערך בתים של קובץ xlsx
    /// </summary>
    Task<byte[]> ExportToExcelAsync(
        ExportReportParams parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// הפקת דוח מותאם אישית ברמת יחידה ארגונית
    /// </summary>
    Task<ReportResult> GetCustomReportAsync(
        CustomReportParams parameters, CancellationToken cancellationToken = default);

    // --- Custom Report Definition CRUD ---

    Task<CustomReportDefinitionDto> CreateCustomReportDefinitionAsync(
        CreateCustomReportDefinitionCommand command, CancellationToken cancellationToken = default);

    Task<CustomReportDefinitionDto> UpdateCustomReportDefinitionAsync(
        UpdateCustomReportDefinitionCommand command, CancellationToken cancellationToken = default);

    Task<IEnumerable<CustomReportDefinitionDto>> GetCustomReportDefinitionsAsync(
        int orgUnitId, CancellationToken cancellationToken = default);

    Task DeleteCustomReportDefinitionAsync(
        int id, CancellationToken cancellationToken = default);
}
