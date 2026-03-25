using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Documents;

public class DocumentService : IDocumentService
{
    private readonly IRepository<Document> _documentRepository;
    private readonly IRepository<RequiredDocument> _requiredDocumentRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<CallForCandidates> _callRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;

    public DocumentService(
        IRepository<Document> documentRepository,
        IRepository<RequiredDocument> requiredDocumentRepository,
        IRepository<Candidacy> candidacyRepository,
        IRepository<CallForCandidates> callRepository,
        IRepository<OrganizationalUnit> orgUnitRepository)
    {
        _documentRepository = documentRepository;
        _requiredDocumentRepository = requiredDocumentRepository;
        _candidacyRepository = candidacyRepository;
        _callRepository = callRepository;
        _orgUnitRepository = orgUnitRepository;
    }

    public async Task<DocumentDto> UploadAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        if (string.IsNullOrWhiteSpace(command.FileName))
            throw new ValidationException("FileName", "שם קובץ הוא שדה חובה");
        if (string.IsNullOrWhiteSpace(command.DocumentType))
            throw new ValidationException("DocumentType", "סוג מסמך הוא שדה חובה");

        // Validate format and size against required document definitions
        await ValidateFormatAndSizeAsync(candidacy, command, cancellationToken);

        // Determine version number - check for existing documents of same type
        var existingDocs = await _documentRepository.FindAsync(
            d => d.CandidacyId == command.CandidacyId && d.DocumentType == command.DocumentType,
            cancellationToken);
        var maxVersion = existingDocs.Any() ? existingDocs.Max(d => d.Version) : 0;

        var entity = new Document
        {
            CandidacyId = command.CandidacyId,
            DocumentType = command.DocumentType,
            FileName = command.FileName,
            BlobUrl = command.BlobUrl,
            ContentType = command.ContentType,
            SizeBytes = command.SizeBytes,
            Status = "Uploaded",
            Version = maxVersion + 1,
            UploadedAt = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<DocumentDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _documentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), id);

        return ToDto(entity);
    }

    public async Task<IEnumerable<DocumentDto>> ListAsync(DocumentQueryParams query, CancellationToken cancellationToken = default)
    {
        var results = await _documentRepository.FindAsync(d =>
            (!query.CandidacyId.HasValue || d.CandidacyId == query.CandidacyId.Value) &&
            (query.DocumentType == null || d.DocumentType == query.DocumentType) &&
            (query.Status == null || d.Status == query.Status),
            cancellationToken);

        return results.Select(ToDto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _documentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), id);

        await _documentRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<DocumentDto> ReviewAsync(ReviewDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _documentRepository.GetByIdAsync(command.DocumentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), command.DocumentId);

        if (command.Status != "Approved" && command.Status != "Rejected")
            throw new ValidationException("Status", "סטטוס חייב להיות 'Approved' או 'Rejected'");

        entity.Status = command.Status;
        entity.ReviewedByUserId = command.ReviewedByUserId;

        await _documentRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<IEnumerable<DocumentVersionDto>> GetVersionHistoryAsync(int candidacyId, string documentType, CancellationToken cancellationToken = default)
    {
        _ = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        var documents = await _documentRepository.FindAsync(
            d => d.CandidacyId == candidacyId && d.DocumentType == documentType,
            cancellationToken);

        return documents.OrderByDescending(d => d.Version).Select(d => new DocumentVersionDto(
            d.Id, d.FileName, d.BlobUrl, d.SizeBytes, d.Status, d.UploadedAt, d.Version));
    }

    // --- Required Document Definitions ---

    public async Task<RequiredDocumentDto> CreateRequiredDocumentAsync(CreateRequiredDocumentCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.DocumentType))
            throw new ValidationException("DocumentType", "סוג מסמך הוא שדה חובה");

        if (command.CallForCandidatesId.HasValue)
        {
            _ = await _callRepository.GetByIdAsync(command.CallForCandidatesId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(CallForCandidates), command.CallForCandidatesId.Value);
        }

        if (command.OrgUnitId.HasValue)
        {
            _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId.Value);
        }

        if (!command.CallForCandidatesId.HasValue && !command.OrgUnitId.HasValue)
            throw new ValidationException("Scope", "יש לציין קול קורא או יחידה ארגונית");

        var entity = new RequiredDocument
        {
            CallForCandidatesId = command.CallForCandidatesId,
            OrgUnitId = command.OrgUnitId,
            DocumentType = command.DocumentType.Trim(),
            IsRequired = command.IsRequired,
            AllowedFormats = command.AllowedFormats,
            MaxSizeKB = command.MaxSizeKB > 0 ? command.MaxSizeKB : 10240
        };

        await _requiredDocumentRepository.AddAsync(entity, cancellationToken);
        return ToRequiredDocumentDto(entity);
    }

    public async Task DeleteRequiredDocumentAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _requiredDocumentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(RequiredDocument), id);

        await _requiredDocumentRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<RequiredDocumentDto>> GetRequiredDocumentsAsync(int? callForCandidatesId, int? orgUnitId, CancellationToken cancellationToken = default)
    {
        var results = await _requiredDocumentRepository.FindAsync(r =>
            (!callForCandidatesId.HasValue || r.CallForCandidatesId == callForCandidatesId.Value) &&
            (!orgUnitId.HasValue || r.OrgUnitId == orgUnitId.Value),
            cancellationToken);

        return results.Select(ToRequiredDocumentDto);
    }

    // --- Completeness Check ---

    public async Task<DocumentCompletenessResult> CheckCompletenessAsync(int candidacyId, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        // Get required documents from both call-level and org-unit-level
        var callRequirements = await _requiredDocumentRepository.FindAsync(
            r => r.CallForCandidatesId == candidacy.CallForCandidatesId, cancellationToken);
        var orgUnitRequirements = await _requiredDocumentRepository.FindAsync(
            r => r.OrgUnitId == candidacy.OrgUnitId && r.CallForCandidatesId == null, cancellationToken);

        var allRequirements = callRequirements.Concat(orgUnitRequirements)
            .GroupBy(r => r.DocumentType)
            .Select(g => g.First()) // deduplicate by document type
            .ToList();

        // Get uploaded documents for this candidacy (latest version per type)
        var uploadedDocs = await _documentRepository.FindAsync(
            d => d.CandidacyId == candidacyId, cancellationToken);

        var latestByType = uploadedDocs
            .GroupBy(d => d.DocumentType)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.Version).First());

        var missingDocs = new List<MissingDocumentInfo>();

        foreach (var req in allRequirements)
        {
            if (!latestByType.TryGetValue(req.DocumentType, out var doc) || doc.Status == "Rejected")
            {
                missingDocs.Add(new MissingDocumentInfo(req.DocumentType, req.IsRequired));
            }
        }

        var isComplete = !missingDocs.Any(m => m.IsRequired);
        return new DocumentCompletenessResult(isComplete, missingDocs);
    }

    // --- Private helpers ---

    private async Task ValidateFormatAndSizeAsync(Candidacy candidacy, UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        // Find matching required document definitions (call-level first, then org-unit-level)
        var requirements = await _requiredDocumentRepository.FindAsync(
            r => r.DocumentType == command.DocumentType &&
                 (r.CallForCandidatesId == candidacy.CallForCandidatesId || r.OrgUnitId == candidacy.OrgUnitId),
            cancellationToken);

        var requirement = requirements.FirstOrDefault();
        if (requirement == null)
            return; // No restrictions defined for this document type

        // Validate format
        if (!string.IsNullOrWhiteSpace(requirement.AllowedFormats))
        {
            var allowedFormats = requirement.AllowedFormats
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(f => f.ToLowerInvariant())
                .ToHashSet();

            var fileExtension = Path.GetExtension(command.FileName)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;

            if (allowedFormats.Count > 0 && !allowedFormats.Contains(fileExtension))
                throw new ValidationException("FileName", $"פורמט הקובץ '{fileExtension}' אינו מותר. פורמטים מותרים: {requirement.AllowedFormats}");
        }

        // Validate size
        if (requirement.MaxSizeKB > 0)
        {
            var fileSizeKB = command.SizeBytes / 1024;
            if (fileSizeKB > requirement.MaxSizeKB)
                throw new ValidationException("SizeBytes", $"גודל הקובץ ({fileSizeKB}KB) חורג מהמקסימום המותר ({requirement.MaxSizeKB}KB)");
        }
    }

    private static DocumentDto ToDto(Document entity) =>
        new(entity.Id, entity.CandidacyId, entity.DocumentType, entity.FileName,
            entity.BlobUrl, entity.ContentType, entity.SizeBytes, entity.Status,
            entity.ReviewedByUserId, entity.UploadedAt, entity.Version);

    private static RequiredDocumentDto ToRequiredDocumentDto(RequiredDocument entity) =>
        new(entity.Id, entity.CallForCandidatesId, entity.OrgUnitId, entity.DocumentType,
            entity.IsRequired, entity.AllowedFormats, entity.MaxSizeKB);
}
