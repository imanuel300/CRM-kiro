using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// יומן גישה למידע אישי - תיעוד כל גישה למידע אישי של מועמדים
/// דרישה: 18.4
/// </summary>
public class PersonalDataAccessLog : BaseEntity
{
    public int UserId { get; set; }
    public int ContactId { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public string FieldsAccessed { get; set; } = string.Empty;
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
