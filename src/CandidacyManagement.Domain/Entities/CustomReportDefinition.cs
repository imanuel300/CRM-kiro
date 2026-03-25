using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// הגדרת דוח מותאם אישית ברמת יחידה ארגונית
/// </summary>
public class CustomReportDefinition : AuditableEntity
{
    public int OrgUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// JSON-serialized list of columns to include in the report
    /// </summary>
    public string ColumnsJson { get; set; } = "[]";

    /// <summary>
    /// JSON-serialized filter definitions for the report
    /// </summary>
    public string FiltersJson { get; set; } = "{}";

    /// <summary>
    /// JSON-serialized grouping/aggregation settings
    /// </summary>
    public string? GroupByJson { get; set; }

    /// <summary>
    /// JSON-serialized sort order settings
    /// </summary>
    public string? SortOrderJson { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
}
