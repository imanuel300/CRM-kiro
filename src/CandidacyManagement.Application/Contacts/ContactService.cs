using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Contacts;

public class ContactService : IContactService
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<ContactChangeHistory> _changeHistoryRepository;
    private readonly IRepository<ContactCustomFieldValue> _customFieldValueRepository;
    private readonly IRepository<CustomFieldDefinition> _customFieldDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ContactService(
        IRepository<Contact> contactRepository,
        IRepository<ContactChangeHistory> changeHistoryRepository,
        IRepository<ContactCustomFieldValue> customFieldValueRepository,
        IRepository<CustomFieldDefinition> customFieldDefinitionRepository,
        IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _changeHistoryRepository = changeHistoryRepository;
        _customFieldValueRepository = customFieldValueRepository;
        _customFieldDefinitionRepository = customFieldDefinitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContactDto> CreateAsync(CreateContactCommand command, CancellationToken cancellationToken = default)
    {
        ValidateIdNumber(command.IdNumber);
        ValidateName(command.FirstName, command.LastName);

        // Check for duplicate IdNumber
        var existing = await FindByIdNumberAsync(command.IdNumber, cancellationToken);
        if (existing != null)
            throw new BusinessRuleViolationException(
                $"איש קשר עם תעודת זהות '{command.IdNumber}' כבר קיים במערכת (מזהה: {existing.Id})");

        var entity = new Contact
        {
            IdNumber = command.IdNumber.Trim(),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            DateOfBirth = command.DateOfBirth,
            Gender = command.Gender,
            Address = command.Address,
            Phone = command.Phone,
            Email = command.Email
        };

        await _contactRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<ContactDto> UpdateAsync(UpdateContactCommand command, CancellationToken cancellationToken = default)
    {
        ValidateName(command.FirstName, command.LastName);

        var entity = await _contactRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.Id);

        // Track changes for audit trail
        var changes = DetectChanges(entity, command);
        foreach (var change in changes)
        {
            change.ChangedByUserId = command.ChangedByUserId;
            await _changeHistoryRepository.AddAsync(change, cancellationToken);
        }

        // Apply updates
        entity.FirstName = command.FirstName.Trim();
        entity.LastName = command.LastName.Trim();
        entity.DateOfBirth = command.DateOfBirth;
        entity.Gender = command.Gender;
        entity.Address = command.Address;
        entity.Phone = command.Phone;
        entity.Email = command.Email;

        await _contactRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<ContactDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _contactRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), id);

        return ToDto(entity);
    }

    public async Task<ContactDto?> GetByIdNumberAsync(string idNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idNumber))
            return null;

        var entity = await FindByIdNumberAsync(idNumber.Trim(), cancellationToken);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<IEnumerable<ContactDto>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            var all = await _contactRepository.GetAllAsync(cancellationToken);
            return all.Select(ToDto);
        }

        var term = searchTerm.Trim();
        var results = await _contactRepository.FindAsync(
            c => c.IdNumber.Contains(term) ||
                 c.FirstName.Contains(term) ||
                 c.LastName.Contains(term) ||
                 (c.Email != null && c.Email.Contains(term)),
            cancellationToken);

        return results.Select(ToDto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _contactRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), id);

        await _contactRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<ChangeHistoryDto>> GetChangeHistoryAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var exists = await _contactRepository.GetByIdAsync(contactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), contactId);

        var history = await _changeHistoryRepository.FindAsync(
            h => h.ContactId == contactId, cancellationToken);

        return history
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new ChangeHistoryDto(h.Id, h.FieldName, h.OldValue, h.NewValue, h.ChangedByUserId, h.ChangedAt));
    }

    public async Task<IEnumerable<CustomFieldValueDto>> GetCustomFieldsAsync(int contactId, int orgUnitId, CancellationToken cancellationToken = default)
    {
        var definitions = await _customFieldDefinitionRepository.FindAsync(
            d => d.OrgUnitId == orgUnitId && d.EntityType == "Contact", cancellationToken);

        var values = await _customFieldValueRepository.FindAsync(
            v => v.ContactId == contactId && v.OrgUnitId == orgUnitId, cancellationToken);

        var valueDict = values.ToDictionary(v => v.CustomFieldDefinitionId);

        return definitions.OrderBy(d => d.SortOrder).Select(d =>
        {
            valueDict.TryGetValue(d.Id, out var val);
            return new CustomFieldValueDto(
                val?.Id ?? 0, d.Id, d.FieldName, d.FieldType, val?.Value, orgUnitId);
        });
    }

    public async Task SetCustomFieldValueAsync(SetCustomFieldValueCommand command, CancellationToken cancellationToken = default)
    {
        // Verify contact exists
        _ = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.ContactId);

        // Verify field definition exists and belongs to the org unit
        var definition = await _customFieldDefinitionRepository.GetByIdAsync(command.CustomFieldDefinitionId, cancellationToken)
            ?? throw new NotFoundException(nameof(CustomFieldDefinition), command.CustomFieldDefinitionId);

        if (definition.OrgUnitId != command.OrgUnitId)
            throw new BusinessRuleViolationException("הגדרת השדה אינה שייכת ליחידה הארגונית המבוקשת");

        // Find existing value or create new
        var existingValues = await _customFieldValueRepository.FindAsync(
            v => v.ContactId == command.ContactId &&
                 v.CustomFieldDefinitionId == command.CustomFieldDefinitionId &&
                 v.OrgUnitId == command.OrgUnitId,
            cancellationToken);

        var existing = existingValues.FirstOrDefault();
        if (existing != null)
        {
            existing.Value = command.Value;
            await _customFieldValueRepository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            var newValue = new ContactCustomFieldValue
            {
                ContactId = command.ContactId,
                CustomFieldDefinitionId = command.CustomFieldDefinitionId,
                OrgUnitId = command.OrgUnitId,
                Value = command.Value
            };
            await _customFieldValueRepository.AddAsync(newValue, cancellationToken);
        }
    }

    // --- Private helpers ---

    private async Task<Contact?> FindByIdNumberAsync(string idNumber, CancellationToken cancellationToken)
    {
        var results = await _contactRepository.FindAsync(
            c => c.IdNumber == idNumber.Trim(), cancellationToken);
        return results.FirstOrDefault();
    }

    private static void ValidateIdNumber(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber))
            throw new ValidationException("IdNumber", "מספר תעודת זהות הוא שדה חובה");
    }

    private static void ValidateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ValidationException("FirstName", "שם פרטי הוא שדה חובה");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ValidationException("LastName", "שם משפחה הוא שדה חובה");
    }

    private static List<ContactChangeHistory> DetectChanges(Contact entity, UpdateContactCommand command)
    {
        var changes = new List<ContactChangeHistory>();
        var contactId = entity.Id;

        if (entity.FirstName != command.FirstName.Trim())
            changes.Add(new ContactChangeHistory { ContactId = contactId, FieldName = "FirstName", OldValue = entity.FirstName, NewValue = command.FirstName.Trim() });

        if (entity.LastName != command.LastName.Trim())
            changes.Add(new ContactChangeHistory { ContactId = contactId, FieldName = "LastName", OldValue = entity.LastName, NewValue = command.LastName.Trim() });

        if (entity.DateOfBirth != command.DateOfBirth)
            changes.Add(new ContactChangeHistory { ContactId = contactId, FieldName = "DateOfBirth", OldValue = entity.DateOfBirth?.ToString("o"), NewValue = command.DateOfBirth?.ToString("o") });

        if (entity.Gender != command.Gender)
            changes.Add(new ContactChangeHistory { ContactId = contactId, FieldName = "Gender", OldValue = entity.Gender, NewValue = command.Gender });

        if (entity.Address != command.Address)
            changes.Add(new ContactChangeHistory { ContactId = contactId, FieldName = "Address", OldValue = entity.Address, NewValue = command.Address });

        if (entity.Phone != command.Phone)
            changes.Add(new ContactChangeHistory { ContactId = contactId, FieldName = "Phone", OldValue = entity.Phone, NewValue = command.Phone });

        if (entity.Email != command.Email)
            changes.Add(new ContactChangeHistory { ContactId = contactId, FieldName = "Email", OldValue = entity.Email, NewValue = command.Email });

        return changes;
    }

    private static ContactDto ToDto(Contact entity) =>
        new(entity.Id, entity.IdNumber, entity.FirstName, entity.LastName,
            entity.DateOfBirth, entity.Gender, entity.Address, entity.Phone,
            entity.Email, entity.CreatedAt, entity.UpdatedAt);
}
