using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Quotas;

public class QuotaService : IQuotaService
{
    private readonly IRepository<Quota> _quotaRepository;
    private readonly IRepository<QuotaAssignment> _assignmentRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;

    public QuotaService(
        IRepository<Quota> quotaRepository,
        IRepository<QuotaAssignment> assignmentRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IRepository<Candidacy> candidacyRepository)
    {
        _quotaRepository = quotaRepository;
        _assignmentRepository = assignmentRepository;
        _orgUnitRepository = orgUnitRepository;
        _candidacyRepository = candidacyRepository;
    }

    public async Task<QuotaDto> CreateAsync(CreateQuotaCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        if (string.IsNullOrWhiteSpace(command.CategoryName))
            throw new ValidationException("CategoryName", "יש לציין שם קטגוריה");

        if (command.TargetCount <= 0)
            throw new ValidationException("TargetCount", "יעד המכסה חייב להיות מספר חיובי");

        // Check for duplicate category in same org unit
        var existing = await _quotaRepository.FindAsync(
            q => q.OrgUnitId == command.OrgUnitId
                 && q.CategoryName == command.CategoryName
                 && q.IsActive,
            cancellationToken);

        if (existing.Any())
            throw new ValidationException("CategoryName", "קיימת מכסה פעילה עם אותו שם קטגוריה ביחידה ארגונית זו");

        var entity = new Quota
        {
            OrgUnitId = command.OrgUnitId,
            CategoryName = command.CategoryName,
            TargetCount = command.TargetCount,
            CurrentCount = 0,
            Description = command.Description,
            IsActive = true
        };

        await _quotaRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<QuotaDto> UpdateAsync(UpdateQuotaCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _quotaRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Quota), command.Id);

        if (string.IsNullOrWhiteSpace(command.CategoryName))
            throw new ValidationException("CategoryName", "יש לציין שם קטגוריה");

        if (command.TargetCount <= 0)
            throw new ValidationException("TargetCount", "יעד המכסה חייב להיות מספר חיובי");

        entity.CategoryName = command.CategoryName;
        entity.TargetCount = command.TargetCount;
        entity.Description = command.Description;
        entity.IsActive = command.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _quotaRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _quotaRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Quota), id);

        await _quotaRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<QuotaDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _quotaRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Quota), id);

        return ToDto(entity);
    }

    public async Task<IEnumerable<QuotaDto>> GetByOrgUnitAsync(int orgUnitId, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(orgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), orgUnitId);

        var results = await _quotaRepository.FindAsync(
            q => q.OrgUnitId == orgUnitId, cancellationToken);

        return results.Select(ToDto);
    }

    public async Task<QuotaAssignmentDto> AssignCandidacyAsync(AssignCandidacyCommand command, CancellationToken cancellationToken = default)
    {
        var quota = await _quotaRepository.GetByIdAsync(command.QuotaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quota), command.QuotaId);

        if (!quota.IsActive)
            throw new ValidationException("QuotaId", "לא ניתן לשייך מועמדות למכסה לא פעילה");

        _ = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        // Check for duplicate assignment
        var existingAssignment = await _assignmentRepository.FindAsync(
            a => a.QuotaId == command.QuotaId && a.CandidacyId == command.CandidacyId,
            cancellationToken);

        if (existingAssignment.Any())
            throw new ValidationException("CandidacyId", "מועמדות זו כבר משויכת למכסה זו");

        var assignment = new QuotaAssignment
        {
            QuotaId = command.QuotaId,
            CandidacyId = command.CandidacyId
        };

        await _assignmentRepository.AddAsync(assignment, cancellationToken);

        quota.CurrentCount++;
        quota.UpdatedAt = DateTime.UtcNow;
        await _quotaRepository.UpdateAsync(quota, cancellationToken);

        return new QuotaAssignmentDto(assignment.Id, assignment.QuotaId, assignment.CandidacyId, assignment.CreatedAt);
    }

    public async Task UnassignCandidacyAsync(UnassignCandidacyCommand command, CancellationToken cancellationToken = default)
    {
        var quota = await _quotaRepository.GetByIdAsync(command.QuotaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quota), command.QuotaId);

        var assignments = await _assignmentRepository.FindAsync(
            a => a.QuotaId == command.QuotaId && a.CandidacyId == command.CandidacyId,
            cancellationToken);

        var assignment = assignments.FirstOrDefault()
            ?? throw new NotFoundException(nameof(QuotaAssignment), $"QuotaId={command.QuotaId}, CandidacyId={command.CandidacyId}");

        await _assignmentRepository.DeleteAsync(assignment, cancellationToken);

        if (quota.CurrentCount > 0)
        {
            quota.CurrentCount--;
            quota.UpdatedAt = DateTime.UtcNow;
            await _quotaRepository.UpdateAsync(quota, cancellationToken);
        }
    }

    public async Task<OrgUnitFulfillmentDto> GetFulfillmentStatusAsync(int orgUnitId, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(orgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), orgUnitId);

        var quotas = await _quotaRepository.FindAsync(
            q => q.OrgUnitId == orgUnitId, cancellationToken);

        var fulfillments = quotas.Select(q => new QuotaFulfillmentDto(
            q.Id,
            q.CategoryName,
            q.TargetCount,
            q.CurrentCount,
            q.TargetCount > 0 ? Math.Round((double)q.CurrentCount / q.TargetCount * 100, 2) : 0,
            q.IsActive));

        return new OrgUnitFulfillmentDto(orgUnitId, fulfillments);
    }

    private static QuotaDto ToDto(Quota entity) =>
        new(entity.Id, entity.OrgUnitId, entity.CategoryName, entity.TargetCount,
            entity.CurrentCount, entity.Description, entity.IsActive, entity.CreatedAt);
}
