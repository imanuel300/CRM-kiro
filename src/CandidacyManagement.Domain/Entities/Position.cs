using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// תפקיד/משרה - משרה המקושרת לקול קורא
/// </summary>
public class Position : BaseEntity
{
    public int CallForCandidatesId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Vacancies { get; set; } = 1;

    // Navigation properties
    public CallForCandidates CallForCandidates { get; set; } = null!;
}
