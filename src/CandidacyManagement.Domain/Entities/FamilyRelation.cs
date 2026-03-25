using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// הצהרת קרבה משפחתית - הצהרה על קשר משפחתי של מועמד לבעלי תפקידים
/// </summary>
public class FamilyRelation : BaseEntity
{
    public int CandidacyId { get; set; }
    public int ContactId { get; set; }
    public string RelationType { get; set; } = string.Empty;
    public string RelatedPersonName { get; set; } = string.Empty;
    public string? RelatedPersonRole { get; set; }
    public bool RequiresManualReview { get; set; }

    // Navigation properties
    public Candidacy Candidacy { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
}
