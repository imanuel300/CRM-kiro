using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// היסטוריית שינויים של איש קשר - תיעוד כל שינוי שדה ברשומת איש קשר
/// </summary>
public class ContactChangeHistory : BaseEntity
{
    public int ContactId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public int? ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Contact Contact { get; set; } = null!;
}
