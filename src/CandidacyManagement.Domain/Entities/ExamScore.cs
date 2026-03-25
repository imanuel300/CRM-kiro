using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// ציון מבחן - ציוני בודקים וציון סופי מחושב למועמד במבחן
/// </summary>
public class ExamScore : BaseEntity
{
    public int ExamId { get; set; }
    public int CandidacyId { get; set; }
    public decimal? FirstExaminerScore { get; set; }
    public decimal? SecondExaminerScore { get; set; }
    public decimal? FinalScore { get; set; }
    public string? ScoreFormula { get; set; }
    public bool? PassedThreshold { get; set; }
    public bool IsAppealed { get; set; }
    public decimal? AppealScore { get; set; }
    public DateTime? ScoredAt { get; set; }

    // Navigation properties
    public Exam Exam { get; set; } = null!;
    public Candidacy Candidacy { get; set; } = null!;
}
