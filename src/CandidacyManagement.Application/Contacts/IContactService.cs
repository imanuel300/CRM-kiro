namespace CandidacyManagement.Application.Contacts;

public interface IContactService
{
    Task<ContactDto> CreateAsync(CreateContactCommand command, CancellationToken cancellationToken = default);
    Task<ContactDto> UpdateAsync(UpdateContactCommand command, CancellationToken cancellationToken = default);
    Task<ContactDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ContactDto?> GetByIdNumberAsync(string idNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<ContactDto>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChangeHistoryDto>> GetChangeHistoryAsync(int contactId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomFieldValueDto>> GetCustomFieldsAsync(int contactId, int orgUnitId, CancellationToken cancellationToken = default);
    Task SetCustomFieldValueAsync(SetCustomFieldValueCommand command, CancellationToken cancellationToken = default);
}
