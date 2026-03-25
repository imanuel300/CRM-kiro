using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// שיוך מועמדות למכסה - קישור בין מועמדות למכסה ספציפית
/// </summary>
public class QuotaAssignment : AuditableEntity
{
    public int QuotaId { get; set; }
    public int CandidacyId { get; set; }

    // Navigation properties
    public Quota Quota { get; set; } = null!;
    public Candidacy Candidacy { get; set; } = null!;
}
