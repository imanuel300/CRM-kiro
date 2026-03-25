namespace CandidacyManagement.Application.Documents;

public record MergeDocumentsCommand(
    IReadOnlyList<int> DocumentIds,
    string OutputFileName = "merged.pdf");

public record MergeDocumentsResult(
    string BlobUrl,
    string FileName,
    int MergedCount);
