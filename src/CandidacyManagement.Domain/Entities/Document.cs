using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// מסמך - קובץ דיגיטלי המצורף למועמדות (תעודה, קורות חיים, הצהרה וכדומה)
/// </summary>
public class Document : BaseEntity
{
    public int CandidacyId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Status { get; set; } = "Uploaded"; // Uploaded, Approved, Rejected
    public int? ReviewedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;

    // Navigation properties
    public Candidacy Candidacy { get; set; } = null!;
}
