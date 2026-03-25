using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// מבחן - שלב מיון הכולל בחינה עם ציון, בודקים ואפשרות ערעור
/// </summary>
public class Exam : BaseEntity
{
    public int OrgUnitId { get; set; }
    public int CallForCandidatesId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public string? Location { get; set; }
    public decimal MaxScore { get; set; }
    public decimal? PassingScore { get; set; }
    public int? FirstExaminerId { get; set; }
    public int? SecondExaminerId { get; set; }
    public DateTime? AppealDeadline { get; set; }

    // Navigation properties
    public OrganizationalUnit OrgUnit { get; set; } = null!;
    public CallForCandidates CallForCandidates { get; set; } = null!;
    public ICollection<ExamScore> Scores { get; set; } = new List<ExamScore>();
}
