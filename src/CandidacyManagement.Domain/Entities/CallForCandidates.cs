using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// קול קורא / מכרז - פרסום פומבי להגשת מועמדויות לתפקיד
/// </summary>
public class CallForCandidates : BaseEntity
{
    public int OrgUnitId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public bool IsTender { get; set; }
    public decimal? MinScore { get; set; }
    public string? EligibilityConditions { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public ICollection<Candidacy> Candidacies { get; set; } = new List<Candidacy>();
    public ICollection<ThresholdCondition> ThresholdConditions { get; set; } = new List<ThresholdCondition>();
    public ICollection<Position> Positions { get; set; } = new List<Position>();
    public ICollection<RequiredDocument> RequiredDocuments { get; set; } = new List<RequiredDocument>();
}
