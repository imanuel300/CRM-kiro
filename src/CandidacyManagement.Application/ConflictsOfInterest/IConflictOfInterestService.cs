namespace CandidacyManagement.Application.ConflictsOfInterest;

public interface IConflictOfInterestService
{
    // ניגוד עניינים - CRUD
    Task<ConflictOfInterestDto> CreateConflictAsync(CreateConflictOfInterestCommand command, CancellationToken cancellationToken = default);
    Task<ConflictOfInterestDto> UpdateConflictAsync(UpdateConflictOfInterestCommand command, CancellationToken cancellationToken = default);
    Task<ConflictOfInterestDto> GetConflictByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteConflictAsync(int id, CancellationToken cancellationToken = default);

    // קרבה משפחתית - CRUD
    Task<FamilyRelationDto> CreateFamilyRelationAsync(CreateFamilyRelationCommand command, CancellationToken cancellationToken = default);
    Task<FamilyRelationDto> UpdateFamilyRelationAsync(UpdateFamilyRelationCommand command, CancellationToken cancellationToken = default);
    Task<FamilyRelationDto> GetFamilyRelationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteFamilyRelationAsync(int id, CancellationToken cancellationToken = default);

    // שליפת כל ההצהרות למועמדות
    Task<CandidacyDeclarationsDto> GetDeclarationsForCandidacyAsync(int candidacyId, CancellationToken cancellationToken = default);

    // סקירת ניגוד עניינים
    Task<ConflictOfInterestDto> ReviewConflictAsync(ReviewConflictCommand command, CancellationToken cancellationToken = default);

    // מועמדויות הדורשות בדיקה ידנית
    Task<IEnumerable<int>> GetCandidacyIdsRequiringManualReviewAsync(ManualReviewQueryParams query, CancellationToken cancellationToken = default);
}
