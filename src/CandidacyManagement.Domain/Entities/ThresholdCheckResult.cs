using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// תוצאת בדיקת תנאי סף - תיעוד תוצאת בדיקה לכל תנאי בנפרד
/// </summary>
public class ThresholdCheckResult : BaseEntity
{
    public int CandidacyId { get; set; }
    public int ThresholdConditionId { get; set; }
    public bool Passed { get; set; }
    public string? ActualValue { get; set; }
    public string? Notes { get; set; }
    public bool IsAutomatic { get; set; }
    public int? CheckedByUserId { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Candidacy Candidacy { get; set; } = null!;
    public ThresholdCondition ThresholdCondition { get; set; } = null!;
}
