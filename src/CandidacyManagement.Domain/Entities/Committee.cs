using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// ועדה - גוף מקבל החלטות המורכב מחברים בעלי תפקידים
/// </summary>
public class Committee : BaseEntity
{
    public int OrgUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// רשימת חברי ועדה ותפקידיהם (JSON-serialized: [{memberId: int, role: string}])
    /// </summary>
    public string MembersJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public ICollection<CommitteeMeeting> Meetings { get; set; } = new List<CommitteeMeeting>();
}
