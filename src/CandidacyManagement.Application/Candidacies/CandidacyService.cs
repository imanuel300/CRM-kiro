using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Candidacies;

public class CandidacyService : ICandidacyService
{
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IRepository<CallForCandidates> _callRepository;
    private readonly IRepository<StatusDefinition> _statusRepository;
    private readonly IRepository<WorkflowDefinition> _workflowRepository;
    private readonly IRepository<CandidacyCustomFieldValue> _customFieldValueRepository;
    private readonly IRepository<CustomFieldDefinition> _customFieldDefinitionRepository;
    private readonly IRepository<StatusTransition> _transitionRepository;
    private readonly IRepository<CandidacyStatusHistory> _historyRepository;

    public CandidacyService(
        IRepository<Candidacy> candidacyRepository,
        IRepository<Contact> contactRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IRepository<CallForCandidates> callRepository,
        IRepository<StatusDefinition> statusRepository,
        IRepository<WorkflowDefinition> workflowRepository,
        IRepository<CandidacyCustomFieldValue> customFieldValueRepository,
        IRepository<CustomFieldDefinition> customFieldDefinitionRepository,
        IRepository<StatusTransition> transitionRepository,
        IRepository<CandidacyStatusHistory> historyRepository)
    {
        _candidacyRepository = candidacyRepository;
        _contactRepository = contactRepository;
        _orgUnitRepository = orgUnitRepository;
        _callRepository = callRepository;
        _statusRepository = statusRepository;
        _workflowRepository = workflowRepository;
        _customFieldValueRepository = customFieldValueRepository;
        _customFieldDefinitionRepository = customFieldDefinitionRepository;
        _transitionRepository = transitionRepository;
        _historyRepository = historyRepository;
    }

    public async Task<CandidacyDto> CreateAsync(CreateCandidacyCommand command, CancellationToken cancellationToken = default)
    {
        // Validate referenced entities exist
        _ = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.ContactId);

        var orgUnit = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        var call = await _callRepository.GetByIdAsync(command.CallForCandidatesId, cancellationToken)
            ?? throw new NotFoundException(nameof(CallForCandidates), command.CallForCandidatesId);

        // Block submissions after close date has passed
        if (call.CloseDate.HasValue && call.CloseDate.Value <= DateTime.UtcNow)
            throw new BusinessRuleViolationException(
                "לא ניתן להגיש מועמדות לקול קורא שתאריך הסגירה שלו חלף");

        // Check for duplicate active candidacy for same contact + call-for-candidates
        var duplicateExists = await _candidacyRepository.ExistsAsync(
            c => c.ContactId == command.ContactId
                 && c.CallForCandidatesId == command.CallForCandidatesId
                 && c.IsActive,
            cancellationToken);

        if (duplicateExists)
            throw new BusinessRuleViolationException(
                "לא ניתן ליצור מועמדות כפולה - קיימת כבר מועמדות פעילה לאיש קשר זה בקול קורא זה");

        // Get initial status from workflow definition
        var initialStatus = await GetInitialStatusAsync(command.OrgUnitId, cancellationToken);

        // Get active workflow version
        var workflowVersion = await GetActiveWorkflowVersionAsync(command.OrgUnitId, cancellationToken);

        var entity = new Candidacy
        {
            ContactId = command.ContactId,
            OrgUnitId = command.OrgUnitId,
            CallForCandidatesId = command.CallForCandidatesId,
            CurrentStatusId = initialStatus?.Id,
            IsActive = true,
            SubmittedAt = DateTime.UtcNow,
            WorkflowDefinitionVersion = workflowVersion
        };

        await _candidacyRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<CandidacyDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _candidacyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), id);

        return ToDto(entity);
    }

    public async Task<CandidacyDetailDto> GetDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _candidacyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), id);

        var customFields = await GetCustomFieldsAsync(id, cancellationToken);

        return new CandidacyDetailDto(
            entity.Id, entity.ContactId, entity.OrgUnitId, entity.CallForCandidatesId,
            entity.CurrentStatusId, entity.CurrentSubStatusId, entity.WorkflowDefinitionVersion,
            entity.IsActive, entity.SubmittedAt, entity.CreatedAt, entity.UpdatedAt,
            customFields);
    }

    public async Task<IEnumerable<CandidacyDto>> ListAsync(CandidacyQueryParams query, CancellationToken cancellationToken = default)
    {
        var results = await _candidacyRepository.FindAsync(c =>
            (!query.OrgUnitId.HasValue || c.OrgUnitId == query.OrgUnitId.Value) &&
            (!query.ContactId.HasValue || c.ContactId == query.ContactId.Value) &&
            (!query.CallForCandidatesId.HasValue || c.CallForCandidatesId == query.CallForCandidatesId.Value) &&
            (!query.IsActive.HasValue || c.IsActive == query.IsActive.Value),
            cancellationToken);

        return results.Select(ToDto);
    }

    public async Task<CandidacyDto> UpdateAsync(UpdateCandidacyCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _candidacyRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.Id);

        entity.CurrentSubStatusId = command.CurrentSubStatusId;

        await _candidacyRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _candidacyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), id);

        await _candidacyRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<CandidacyCustomFieldValueDto>> GetCustomFieldsAsync(int candidacyId, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        var definitions = await _customFieldDefinitionRepository.FindAsync(
            d => d.OrgUnitId == candidacy.OrgUnitId && d.EntityType == "Candidacy", cancellationToken);

        var values = await _customFieldValueRepository.FindAsync(
            v => v.CandidacyId == candidacyId, cancellationToken);

        var valueDict = values.ToDictionary(v => v.CustomFieldDefinitionId);

        return definitions.OrderBy(d => d.SortOrder).Select(d =>
        {
            valueDict.TryGetValue(d.Id, out var val);
            return new CandidacyCustomFieldValueDto(
                val?.Id ?? 0, d.Id, d.FieldName, d.FieldType, val?.Value);
        });
    }

    public async Task SetCustomFieldValueAsync(SetCandidacyCustomFieldValueCommand command, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        var definition = await _customFieldDefinitionRepository.GetByIdAsync(command.CustomFieldDefinitionId, cancellationToken)
            ?? throw new NotFoundException(nameof(CustomFieldDefinition), command.CustomFieldDefinitionId);

        if (definition.OrgUnitId != candidacy.OrgUnitId || definition.EntityType != "Candidacy")
            throw new BusinessRuleViolationException("הגדרת השדה אינה שייכת למועמדויות ביחידה הארגונית של מועמדות זו");

        var existingValues = await _customFieldValueRepository.FindAsync(
            v => v.CandidacyId == command.CandidacyId && v.CustomFieldDefinitionId == command.CustomFieldDefinitionId,
            cancellationToken);

        var existing = existingValues.FirstOrDefault();
        if (existing != null)
        {
            existing.Value = command.Value;
            await _customFieldValueRepository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            var newValue = new CandidacyCustomFieldValue
            {
                CandidacyId = command.CandidacyId,
                CustomFieldDefinitionId = command.CustomFieldDefinitionId,
                Value = command.Value
            };
            await _customFieldValueRepository.AddAsync(newValue, cancellationToken);
        }
    }

    // --- Status Transition ---

    public async Task<CandidacyDto> TransitionStatusAsync(TransitionStatusCommand command, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        // Check if candidacy is already in a final status
        if (candidacy.CurrentStatusId.HasValue)
        {
            var currentStatus = await _statusRepository.GetByIdAsync(candidacy.CurrentStatusId.Value, cancellationToken);
            if (currentStatus != null && currentStatus.IsFinal)
                throw new BusinessRuleViolationException("מועמדות בסטטוס סופי אינה ניתנת לשינוי");
        }

        if (!candidacy.IsActive)
            throw new BusinessRuleViolationException("מועמדות זו אינה פעילה");

        // Validate the target status exists
        var newStatus = await _statusRepository.GetByIdAsync(command.NewStatusId, cancellationToken)
            ?? throw new NotFoundException(nameof(StatusDefinition), command.NewStatusId);

        // Validate the transition is allowed
        var transitions = await _transitionRepository.FindAsync(
            t => t.OrgUnitId == candidacy.OrgUnitId
                 && t.FromStatusId == candidacy.CurrentStatusId
                 && t.ToStatusId == command.NewStatusId,
            cancellationToken);

        if (!transitions.Any())
            throw new BusinessRuleViolationException("מעבר סטטוס זה אינו מותר");

        // Record history
        var history = new CandidacyStatusHistory
        {
            CandidacyId = candidacy.Id,
            FromStatusId = candidacy.CurrentStatusId,
            ToStatusId = command.NewStatusId,
            Reason = command.Reason,
            ChangedByUserId = command.UserId,
            ChangedAt = DateTime.UtcNow
        };

        // Update candidacy status
        candidacy.CurrentStatusId = command.NewStatusId;

        // If the new status is final, mark candidacy as inactive
        if (newStatus.IsFinal)
            candidacy.IsActive = false;

        await _candidacyRepository.UpdateAsync(candidacy, cancellationToken);
        await _historyRepository.AddAsync(history, cancellationToken);

        return ToDto(candidacy);
    }

    public async Task<IEnumerable<StatusHistoryDto>> GetStatusHistoryAsync(int candidacyId, CancellationToken cancellationToken = default)
    {
        _ = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        var history = await _historyRepository.FindAsync(
            h => h.CandidacyId == candidacyId, cancellationToken);

        return history.OrderByDescending(h => h.ChangedAt).Select(h => new StatusHistoryDto(
            h.Id, h.CandidacyId, h.FromStatusId, h.ToStatusId,
            h.FromSubStatusId, h.ToSubStatusId, h.Reason,
            h.ChangedByUserId, h.ChangedAt));
    }

    // --- Private helpers ---

    private async Task<StatusDefinition?> GetInitialStatusAsync(int orgUnitId, CancellationToken cancellationToken)
    {
        var statuses = await _statusRepository.FindAsync(
            s => s.OrgUnitId == orgUnitId && s.IsInitial, cancellationToken);
        return statuses.FirstOrDefault();
    }

    private async Task<int?> GetActiveWorkflowVersionAsync(int orgUnitId, CancellationToken cancellationToken)
    {
        var workflows = await _workflowRepository.FindAsync(
            w => w.OrgUnitId == orgUnitId && w.IsActive, cancellationToken);
        return workflows.OrderByDescending(w => w.Version).FirstOrDefault()?.Version;
    }

    private static CandidacyDto ToDto(Candidacy entity) =>
        new(entity.Id, entity.ContactId, entity.OrgUnitId, entity.CallForCandidatesId,
            entity.CurrentStatusId, entity.CurrentSubStatusId, entity.WorkflowDefinitionVersion,
            entity.IsActive, entity.SubmittedAt, entity.CreatedAt, entity.UpdatedAt);
}
