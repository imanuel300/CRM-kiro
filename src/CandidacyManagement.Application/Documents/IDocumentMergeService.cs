namespace CandidacyManagement.Application.Documents;

public interface IDocumentMergeService
{
    /// <summary>
    /// Merges multiple documents (by ID) into a single PDF and returns the merged blob URL.
    /// </summary>
    Task<MergeDocumentsResult> MergeAsync(MergeDocumentsCommand command, CancellationToken cancellationToken = default);
}
