using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.ConflictsOfInterest;

public class ConflictOfInterestService : IConflictOfInterestService
{
    private readonly IRepository<ConflictOfInterest> _conflictRepository;
    private readonly IRepository<FamilyRelation> _familyRelationRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<Contact> _contactRepository;

    public ConflictOfInterestService(
        IRepository<ConflictOfInterest> conflictRepository,
        IRepository<FamilyRelation> familyRelationRepository,
        IRepository<Candidacy> candidacyRepository,
        IRepository<Contact> contactRepository)
    {
        _conflictRepository = conflictRepository;
        _familyRelationRepository = familyRelationRepository;
        _candidacyRepository = candidacyRepository;
        _contactRepository = contactRepository;
    }

    // --- ניגוד עניינים ---

    public async Task<ConflictOfInterestDto> CreateConflictAsync(
        CreateConflictOfInterestCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        _ = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.ContactId);

        if (string.IsNullOrWhiteSpace(command.QuestionnaireResponses))
            throw new ValidationException("QuestionnaireResponses", "יש למלא את תשובות השאלון");

        var entity = new ConflictOfInterest
        {
            CandidacyId = command.CandidacyId,
            ContactId = command.ContactId,
            QuestionnaireResponses = command.QuestionnaireResponses,
            HasConflict = command.HasConflict,
            RequiresManualReview = command.HasConflict // סימון לבדיקה ידנית אם יש ניגוד
        };

        await _conflictRepository.AddAsync(entity, cancellationToken);
        return ToConflictDto(entity);
    }

    public async Task<ConflictOfInterestDto> UpdateConflictAsync(
        UpdateConflictOfInterestCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _conflictRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ConflictOfInterest), command.Id);

        if (string.IsNullOrWhiteSpace(command.QuestionnaireResponses))
            throw new ValidationException("QuestionnaireResponses", "יש למלא את תשובות השאלון");

        entity.QuestionnaireResponses = command.QuestionnaireResponses;
        entity.HasConflict = command.HasConflict;
        entity.RequiresManualReview = command.HasConflict;

        // איפוס סקירה קודמת כאשר ההצהרה משתנה
        entity.ReviewedByUserId = null;
        entity.ReviewedAt = null;

        await _conflictRepository.UpdateAsync(entity, cancellationToken);
        return ToConflictDto(entity);
    }

    public async Task<ConflictOfInterestDto> GetConflictByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var entity = await _conflictRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(ConflictOfInterest), id);

        return ToConflictDto(entity);
    }

    public async Task DeleteConflictAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _conflictRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(ConflictOfInterest), id);

        await _conflictRepository.DeleteAsync(entity, cancellationToken);
    }

    // --- קרבה משפחתית ---

    public async Task<FamilyRelationDto> CreateFamilyRelationAsync(
        CreateFamilyRelationCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        _ = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.ContactId);

        if (string.IsNullOrWhiteSpace(command.RelationType))
            throw new ValidationException("RelationType", "יש לציין סוג קרבה");

        if (string.IsNullOrWhiteSpace(command.RelatedPersonName))
            throw new ValidationException("RelatedPersonName", "יש לציין שם הקרוב");

        var entity = new FamilyRelation
        {
            CandidacyId = command.CandidacyId,
            ContactId = command.ContactId,
            RelationType = command.RelationType,
            RelatedPersonName = command.RelatedPersonName,
            RelatedPersonRole = command.RelatedPersonRole,
            RequiresManualReview = true // כל הצהרת קרבה דורשת בדיקה ידנית
        };

        await _familyRelationRepository.AddAsync(entity, cancellationToken);
        return ToFamilyRelationDto(entity);
    }

    public async Task<FamilyRelationDto> UpdateFamilyRelationAsync(
        UpdateFamilyRelationCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _familyRelationRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyRelation), command.Id);

        if (string.IsNullOrWhiteSpace(command.RelationType))
            throw new ValidationException("RelationType", "יש לציין סוג קרבה");

        if (string.IsNullOrWhiteSpace(command.RelatedPersonName))
            throw new ValidationException("RelatedPersonName", "יש לציין שם הקרוב");

        entity.RelationType = command.RelationType;
        entity.RelatedPersonName = command.RelatedPersonName;
        entity.RelatedPersonRole = command.RelatedPersonRole;

        await _familyRelationRepository.UpdateAsync(entity, cancellationToken);
        return ToFamilyRelationDto(entity);
    }

    public async Task<FamilyRelationDto> GetFamilyRelationByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var entity = await _familyRelationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyRelation), id);

        return ToFamilyRelationDto(entity);
    }

    public async Task DeleteFamilyRelationAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _familyRelationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyRelation), id);

        await _familyRelationRepository.DeleteAsync(entity, cancellationToken);
    }

    // --- שליפת כל ההצהרות למועמדות ---

    public async Task<CandidacyDeclarationsDto> GetDeclarationsForCandidacyAsync(
        int candidacyId, CancellationToken cancellationToken = default)
    {
        _ = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        var conflicts = await _conflictRepository.FindAsync(
            c => c.CandidacyId == candidacyId, cancellationToken);

        var familyRelations = await _familyRelationRepository.FindAsync(
            f => f.CandidacyId == candidacyId, cancellationToken);

        return new CandidacyDeclarationsDto(
            candidacyId,
            conflicts.Select(ToConflictDto).ToList(),
            familyRelations.Select(ToFamilyRelationDto).ToList());
    }

    // --- סקירת ניגוד עניינים ---

    public async Task<ConflictOfInterestDto> ReviewConflictAsync(
        ReviewConflictCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _conflictRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ConflictOfInterest), command.Id);

        entity.ReviewedByUserId = command.ReviewedByUserId;
        entity.ReviewedAt = DateTime.UtcNow;
        entity.RequiresManualReview = false;

        await _conflictRepository.UpdateAsync(entity, cancellationToken);
        return ToConflictDto(entity);
    }

    // --- מועמדויות הדורשות בדיקה ידנית ---

    public async Task<IEnumerable<int>> GetCandidacyIdsRequiringManualReviewAsync(
        ManualReviewQueryParams query, CancellationToken cancellationToken = default)
    {
        // מועמדויות עם ניגוד עניינים הדורש בדיקה
        var conflictFlags = await _conflictRepository.FindAsync(
            c => c.RequiresManualReview, cancellationToken);

        // מועמדויות עם קרבה משפחתית הדורשת בדיקה
        var familyFlags = await _familyRelationRepository.FindAsync(
            f => f.RequiresManualReview, cancellationToken);

        var candidacyIds = conflictFlags.Select(c => c.CandidacyId)
            .Union(familyFlags.Select(f => f.CandidacyId))
            .Distinct();

        // סינון לפי יחידה ארגונית אם צוין
        if (query.OrgUnitId.HasValue)
        {
            var candidacies = await _candidacyRepository.FindAsync(
                c => candidacyIds.Contains(c.Id) && c.OrgUnitId == query.OrgUnitId.Value,
                cancellationToken);
            return candidacies.Select(c => c.Id);
        }

        return candidacyIds;
    }

    // --- Mapping helpers ---

    private static ConflictOfInterestDto ToConflictDto(ConflictOfInterest entity) =>
        new(entity.Id, entity.CandidacyId, entity.ContactId,
            entity.QuestionnaireResponses, entity.HasConflict,
            entity.RequiresManualReview, entity.ReviewedByUserId, entity.ReviewedAt);

    private static FamilyRelationDto ToFamilyRelationDto(FamilyRelation entity) =>
        new(entity.Id, entity.CandidacyId, entity.ContactId,
            entity.RelationType, entity.RelatedPersonName,
            entity.RelatedPersonRole, entity.RequiresManualReview);
}
