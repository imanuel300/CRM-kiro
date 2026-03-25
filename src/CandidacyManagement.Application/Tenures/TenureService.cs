using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Tenures;

public class TenureService : ITenureService
{
    private readonly IRepository<Tenure> _tenureRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;

    public TenureService(
        IRepository<Tenure> tenureRepository,
        IRepository<Contact> contactRepository,
        IRepository<OrganizationalUnit> orgUnitRepository)
    {
        _tenureRepository = tenureRepository;
        _contactRepository = contactRepository;
        _orgUnitRepository = orgUnitRepository;
    }

    public async Task<TenureDto> CreateAsync(CreateTenureCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.ContactId);

        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        if (string.IsNullOrWhiteSpace(command.Position))
            throw new ValidationException("Position", "יש לציין תפקיד");

        if (command.ExpectedEndDate <= command.StartDate)
            throw new ValidationException("ExpectedEndDate", "תאריך סיום צפוי חייב להיות אחרי תאריך התחלה");

        // Check for overlapping active tenure for same contact in same org unit with same position
        var existingTenures = await _tenureRepository.FindAsync(
            t => t.ContactId == command.ContactId
                 && t.OrgUnitId == command.OrgUnitId
                 && t.Position == command.Position
                 && t.IsActive,
            cancellationToken);

        if (existingTenures.Any())
            throw new ValidationException("ContactId", "קיימת כהונה פעילה לאיש קשר זה באותו תפקיד ויחידה ארגונית");

        var entity = new Tenure
        {
            ContactId = command.ContactId,
            OrgUnitId = command.OrgUnitId,
            Position = command.Position,
            StartDate = command.StartDate,
            ExpectedEndDate = command.ExpectedEndDate,
            Notes = command.Notes,
            IsActive = true
        };

        await _tenureRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<TenureDto> UpdateAsync(UpdateTenureCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _tenureRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenure), command.Id);

        if (!entity.IsActive)
            throw new ValidationException("Id", "לא ניתן לעדכן כהונה שהסתיימה");

        if (string.IsNullOrWhiteSpace(command.Position))
            throw new ValidationException("Position", "יש לציין תפקיד");

        if (command.ExpectedEndDate <= command.StartDate)
            throw new ValidationException("ExpectedEndDate", "תאריך סיום צפוי חייב להיות אחרי תאריך התחלה");

        entity.Position = command.Position;
        entity.StartDate = command.StartDate;
        entity.ExpectedEndDate = command.ExpectedEndDate;
        entity.Notes = command.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _tenureRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<TenureDto> EndTenureAsync(EndTenureCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _tenureRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenure), command.Id);

        if (!entity.IsActive)
            throw new ValidationException("Id", "כהונה זו כבר הסתיימה");

        entity.EndReason = command.EndReason;
        entity.ActualEndDate = command.ActualEndDate ?? DateTime.UtcNow;
        entity.IsActive = false;
        entity.Notes = command.Notes ?? entity.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _tenureRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<TenureDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _tenureRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenure), id);

        return ToDto(entity);
    }

    public async Task<IEnumerable<TenureDto>> GetByContactAsync(int contactId, CancellationToken cancellationToken = default)
    {
        _ = await _contactRepository.GetByIdAsync(contactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), contactId);

        var results = await _tenureRepository.FindAsync(
            t => t.ContactId == contactId, cancellationToken);

        return results.Select(ToDto);
    }

    public async Task<IEnumerable<TenureDto>> GetByOrgUnitAsync(int orgUnitId, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(orgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), orgUnitId);

        var results = await _tenureRepository.FindAsync(
            t => t.OrgUnitId == orgUnitId, cancellationToken);

        return results.Select(ToDto);
    }

    public async Task<IEnumerable<ExpiringTenureDto>> GetExpiringTenuresAsync(
        int daysThreshold, int? orgUnitId = null, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysThreshold);

        var results = await _tenureRepository.FindAsync(
            t => t.IsActive
                 && t.ExpectedEndDate <= cutoffDate
                 && (!orgUnitId.HasValue || t.OrgUnitId == orgUnitId.Value),
            cancellationToken);

        return results.Select(t => new ExpiringTenureDto(
            t.Id,
            t.ContactId,
            t.OrgUnitId,
            t.Position,
            t.ExpectedEndDate,
            (int)(t.ExpectedEndDate - DateTime.UtcNow).TotalDays));
    }

    public async Task<IEnumerable<TenureDto>> GetHistoryAsync(int contactId, CancellationToken cancellationToken = default)
    {
        _ = await _contactRepository.GetByIdAsync(contactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), contactId);

        var results = await _tenureRepository.FindAsync(
            t => t.ContactId == contactId && !t.IsActive, cancellationToken);

        return results.Select(ToDto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _tenureRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenure), id);

        await _tenureRepository.DeleteAsync(entity, cancellationToken);
    }

    private static TenureDto ToDto(Tenure entity) =>
        new(entity.Id, entity.ContactId, entity.OrgUnitId, entity.Position,
            entity.StartDate, entity.ExpectedEndDate, entity.ActualEndDate,
            entity.EndReason, entity.IsActive, entity.Notes, entity.CreatedAt);
}
