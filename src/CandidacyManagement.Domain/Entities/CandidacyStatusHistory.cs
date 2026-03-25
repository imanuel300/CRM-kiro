using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// היסטוריית שינויי סטטוס מועמדות - תיעוד כל מעבר סטטוס
/// </summary>
public class CandidacyStatusHistory : BaseEntity
{
    public int CandidacyId { get; set; }
    public int? FromStatusId { get; set; }
    public int ToStatusId { get; set; }
    public int? FromSubStatusId { get; set; }
    public int? ToSubStatusId { get; set; }
    public string? Reason { get; set; }
    public int ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Candidacy Candidacy { get; set; } = null!;
    public StatusDefinition? FromStatus { get; set; }
    public StatusDefinition ToStatus { get; set; } = null!;
}
