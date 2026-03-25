namespace CandidacyManagement.Application.Documents;

public interface IDocumentService
{
    // Document CRUD
    Task<DocumentDto> UploadAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default);
    Task<DocumentDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> ListAsync(DocumentQueryParams query, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Review (approve/reject)
    Task<DocumentDto> ReviewAsync(ReviewDocumentCommand command, CancellationToken cancellationToken = default);

    // Version history
    Task<IEnumerable<DocumentVersionDto>> GetVersionHistoryAsync(int candidacyId, string documentType, CancellationToken cancellationToken = default);

    // Required document definitions
    Task<RequiredDocumentDto> CreateRequiredDocumentAsync(CreateRequiredDocumentCommand command, CancellationToken cancellationToken = default);
    Task DeleteRequiredDocumentAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RequiredDocumentDto>> GetRequiredDocumentsAsync(int? callForCandidatesId, int? orgUnitId, CancellationToken cancellationToken = default);

    // Completeness check
    Task<DocumentCompletenessResult> CheckCompletenessAsync(int candidacyId, CancellationToken cancellationToken = default);
}
