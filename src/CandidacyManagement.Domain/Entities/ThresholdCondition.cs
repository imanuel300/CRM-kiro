using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// תנאי סף - תנאי שמועמד חייב לעמוד בו כדי להתקבל לקול קורא
/// </summary>
public class ThresholdCondition : BaseEntity
{
    public int CallForCandidatesId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsAutomatic { get; set; }
    public ConditionType ConditionType { get; set; } = ConditionType.Custom;

    // Navigation properties
    public CallForCandidates CallForCandidates { get; set; } = null!;
    public ICollection<ThresholdCheckResult> CheckResults { get; set; } = new List<ThresholdCheckResult>();
}
