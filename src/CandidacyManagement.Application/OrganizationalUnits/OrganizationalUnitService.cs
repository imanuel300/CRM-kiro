using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.OrganizationalUnits;

public class OrganizationalUnitService : IOrganizationalUnitService
{
    private readonly IRepository<OrganizationalUnit> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public OrganizationalUnitService(IRepository<OrganizationalUnit> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrgUnitDto> CreateAsync(CreateOrgUnitCommand command, CancellationToken cancellationToken = default)
    {
        ValidateName(command.Name);
        await EnsureNameIsUniqueAsync(command.Name, excludeId: null, cancellationToken);

        var entity = new OrganizationalUnit
        {
            Name = command.Name.Trim(),
            Description = command.Description,
            ContactEmail = command.ContactEmail,
            ContactPhone = command.ContactPhone,
            IsActive = true
        };

        await _repository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<OrgUnitDto> UpdateAsync(UpdateOrgUnitCommand command, CancellationToken cancellationToken = default)
    {
        ValidateName(command.Name);

        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.Id);

        await EnsureNameIsUniqueAsync(command.Name, excludeId: command.Id, cancellationToken);

        entity.Name = command.Name.Trim();
        entity.Description = command.Description;
        entity.ContactEmail = command.ContactEmail;
        entity.ContactPhone = command.ContactPhone;

        await _repository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), id);

        entity.IsActive = false;
        await _repository.UpdateAsync(entity, cancellationToken);
    }

    public async Task<OrgUnitDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), id);

        return ToDto(entity);
    }

    public async Task<IEnumerable<OrgUnitDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.FindAsync(ou => ou.IsActive, cancellationToken);
        return entities.Select(ToDto);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "שם היחידה הארגונית הוא שדה חובה");
    }

    private async Task EnsureNameIsUniqueAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var trimmedName = name.Trim();
        var exists = await _repository.ExistsAsync(
            ou => ou.Name == trimmedName && ou.IsActive && (excludeId == null || ou.Id != excludeId),
            cancellationToken);

        if (exists)
            throw new BusinessRuleViolationException($"יחידה ארגונית עם השם '{trimmedName}' כבר קיימת במערכת");
    }

    private static OrgUnitDto ToDto(OrganizationalUnit entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.ContactEmail,
            entity.ContactPhone, entity.IsActive, entity.CreatedAt, entity.UpdatedAt);
}
