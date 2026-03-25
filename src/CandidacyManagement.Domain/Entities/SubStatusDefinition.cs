using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// הגדרת תת-סטטוס - פירוט נוסף של סטטוס מועמדות
/// </summary>
public class SubStatusDefinition : BaseEntity
{
    public int StatusDefinitionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    // Navigation properties
    public StatusDefinition StatusDefinition { get; set; } = null!;
}
