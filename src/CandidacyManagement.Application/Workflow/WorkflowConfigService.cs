using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Workflow;

/// <summary>
/// מימוש שירות הגדרת תהליכי מיון
/// שינוי תהליך יוצר גרסה חדשה - מועמדויות קיימות שומרות על הגרסה שלהן
/// </summary>
public class WorkflowConfigService : IWorkflowConfigService
{
    private readonly IRepository<WorkflowDefinition> _workflowRepo;
    private readonly IRepository<StatusDefinition> _statusRepo;
    private readonly IRepository<SubStatusDefinition> _subStatusRepo;
    private readonly IRepository<StatusTransition> _transitionRepo;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepo;

    public WorkflowConfigService(
        IRepository<WorkflowDefinition> workflowRepo,
        IRepository<StatusDefinition> statusRepo,
        IRepository<SubStatusDefinition> subStatusRepo,
        IRepository<StatusTransition> transitionRepo,
        IRepository<OrganizationalUnit> orgUnitRepo)
    {
        _workflowRepo = workflowRepo;
        _statusRepo = statusRepo;
        _subStatusRepo = subStatusRepo;
        _transitionRepo = transitionRepo;
        _orgUnitRepo = orgUnitRepo;
    }

    public async Task<WorkflowDefinitionDto> ConfigureWorkflowAsync(
        ConfigureWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        await ValidateOrgUnitExistsAsync(command.OrgUnitId, cancellationToken);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "שם תהליך המיון הוא שדה חובה");

        // Deactivate existing active workflows for this org unit
        var existingWorkflows = await _workflowRepo.FindAsync(
            w => w.OrgUnitId == command.OrgUnitId && w.IsActive,
            cancellationToken);

        var latestVersion = 0;
        foreach (var existing in existingWorkflows)
        {
            if (existing.Version > latestVersion)
                latestVersion = existing.Version;
            existing.IsActive = false;
            await _workflowRepo.UpdateAsync(existing, cancellationToken);
        }

        // Create new version
        var workflow = new WorkflowDefinition
        {
            OrgUnitId = command.OrgUnitId,
            Name = command.Name.Trim(),
            ExamStepEnabled = command.ExamStepEnabled,
            InterviewStepEnabled = command.InterviewStepEnabled,
            CommitteeStepEnabled = command.CommitteeStepEnabled,
            ThresholdCheckEnabled = command.ThresholdCheckEnabled,
            StepOrder = command.StepOrder,
            Version = latestVersion + 1,
            IsActive = true
        };

        await _workflowRepo.AddAsync(workflow, cancellationToken);

        return new WorkflowDefinitionDto(
            workflow.Id, workflow.OrgUnitId, workflow.Name,
            workflow.ExamStepEnabled, workflow.InterviewStepEnabled,
            workflow.CommitteeStepEnabled, workflow.ThresholdCheckEnabled,
            workflow.StepOrder, workflow.Version, workflow.IsActive, workflow.CreatedAt);
    }

    public async Task<IEnumerable<StatusDefinitionDto>> ConfigureStatusesAsync(
        ConfigureStatusesCommand command, CancellationToken cancellationToken = default)
    {
        await ValidateOrgUnitExistsAsync(command.OrgUnitId, cancellationToken);
        ValidateStatuses(command.Statuses);

        // Remove existing statuses for this org unit
        var existingStatuses = await _statusRepo.FindAsync(
            s => s.OrgUnitId == command.OrgUnitId, cancellationToken);

        foreach (var existing in existingStatuses)
        {
            // Remove sub-statuses first
            var subStatuses = await _subStatusRepo.FindAsync(
                ss => ss.StatusDefinitionId == existing.Id, cancellationToken);
            foreach (var sub in subStatuses)
                await _subStatusRepo.DeleteAsync(sub, cancellationToken);

            await _statusRepo.DeleteAsync(existing, cancellationToken);
        }

        // Create new statuses
        var result = new List<StatusDefinitionDto>();
        foreach (var statusDef in command.Statuses)
        {
            var status = new StatusDefinition
            {
                OrgUnitId = command.OrgUnitId,
                Code = statusDef.Code.Trim(),
                DisplayName = statusDef.DisplayName.Trim(),
                Category = statusDef.Category,
                IsFinal = statusDef.IsFinal,
                IsInitial = statusDef.IsInitial,
                SortOrder = statusDef.SortOrder
            };

            await _statusRepo.AddAsync(status, cancellationToken);

            var subDtos = new List<SubStatusDefinitionDto>();
            if (statusDef.SubStatuses != null)
            {
                foreach (var subDef in statusDef.SubStatuses)
                {
                    var sub = new SubStatusDefinition
                    {
                        StatusDefinitionId = status.Id,
                        Code = subDef.Code.Trim(),
                        DisplayName = subDef.DisplayName.Trim()
                    };
                    await _subStatusRepo.AddAsync(sub, cancellationToken);
                    subDtos.Add(new SubStatusDefinitionDto(sub.Id, sub.Code, sub.DisplayName));
                }
            }

            result.Add(new StatusDefinitionDto(
                status.Id, status.OrgUnitId, status.Code, status.DisplayName,
                status.Category, status.IsFinal, status.IsInitial, status.SortOrder, subDtos));
        }

        return result;
    }

    public async Task<IEnumerable<StatusTransitionDto>> ConfigureTransitionsAsync(
        ConfigureTransitionsCommand command, CancellationToken cancellationToken = default)
    {
        await ValidateOrgUnitExistsAsync(command.OrgUnitId, cancellationToken);

        // Remove existing transitions
        var existingTransitions = await _transitionRepo.FindAsync(
            t => t.OrgUnitId == command.OrgUnitId, cancellationToken);

        foreach (var existing in existingTransitions)
            await _transitionRepo.DeleteAsync(existing, cancellationToken);

        // Get all statuses for this org unit for code-to-id lookup
        var statuses = await _statusRepo.FindAsync(
            s => s.OrgUnitId == command.OrgUnitId, cancellationToken);
        var statusByCode = statuses.ToDictionary(s => s.Code, s => s);

        var result = new List<StatusTransitionDto>();
        foreach (var transDef in command.Transitions)
        {
            if (!statusByCode.TryGetValue(transDef.FromStatusCode, out var fromStatus))
                throw new ValidationException("FromStatusCode", $"סטטוס מקור '{transDef.FromStatusCode}' לא נמצא");

            if (!statusByCode.TryGetValue(transDef.ToStatusCode, out var toStatus))
                throw new ValidationException("ToStatusCode", $"סטטוס יעד '{transDef.ToStatusCode}' לא נמצא");

            var transition = new StatusTransition
            {
                OrgUnitId = command.OrgUnitId,
                FromStatusId = fromStatus.Id,
                ToStatusId = toStatus.Id,
                RequiredPermission = transDef.RequiredPermission,
                RequiresReason = transDef.RequiresReason,
                AutoTriggerRule = transDef.AutoTriggerRule
            };

            await _transitionRepo.AddAsync(transition, cancellationToken);

            result.Add(new StatusTransitionDto(
                transition.Id, transition.FromStatusId, transition.ToStatusId,
                transDef.FromStatusCode, transDef.ToStatusCode,
                transition.RequiredPermission, transition.RequiresReason, transition.AutoTriggerRule));
        }

        return result;
    }

    private async Task ValidateOrgUnitExistsAsync(int orgUnitId, CancellationToken cancellationToken)
    {
        var orgUnit = await _orgUnitRepo.GetByIdAsync(orgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), orgUnitId);

        if (!orgUnit.IsActive)
            throw new BusinessRuleViolationException("היחידה הארגונית אינה פעילה");
    }

    private static void ValidateStatuses(IEnumerable<ConfigureStatusDefinition> statuses)
    {
        var statusList = statuses.ToList();

        if (!statusList.Any())
            throw new ValidationException("Statuses", "יש להגדיר לפחות סטטוס אחד");

        var initialCount = statusList.Count(s => s.IsInitial);
        if (initialCount != 1)
            throw new ValidationException("Statuses", "יש להגדיר בדיוק סטטוס התחלתי אחד");

        var codes = statusList.Select(s => s.Code.Trim()).ToList();
        if (codes.Distinct().Count() != codes.Count)
            throw new ValidationException("Statuses", "קודי סטטוס חייבים להיות ייחודיים");
    }
}
