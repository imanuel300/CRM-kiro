using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Events;
using CandidacyManagement.Domain.Exceptions;
using MediatR;

namespace CandidacyManagement.Application.Workflow;

/// <summary>
/// מימוש מנוע תהליך מיון - State Machine Engine
/// </summary>
public class WorkflowEngine : IWorkflowEngine
{
    private readonly IRepository<Candidacy> _candidacyRepo;
    private readonly IRepository<StatusDefinition> _statusRepo;
    private readonly IRepository<StatusTransition> _transitionRepo;
    private readonly IRepository<CandidacyStatusHistory> _historyRepo;
    private readonly IRepository<WorkflowDefinition> _workflowRepo;
    private readonly IMediator _mediator;

    public WorkflowEngine(
        IRepository<Candidacy> candidacyRepo,
        IRepository<StatusDefinition> statusRepo,
        IRepository<StatusTransition> transitionRepo,
        IRepository<CandidacyStatusHistory> historyRepo,
        IRepository<WorkflowDefinition> workflowRepo,
        IMediator mediator)
    {
        _candidacyRepo = candidacyRepo;
        _statusRepo = statusRepo;
        _transitionRepo = transitionRepo;
        _historyRepo = historyRepo;
        _workflowRepo = workflowRepo;
        _mediator = mediator;
    }

    public async Task<bool> CanTransitionAsync(int candidacyId, string targetStatusCode, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepo.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        if (!candidacy.IsActive || candidacy.CurrentStatusId == null)
            return false;

        var currentStatus = await _statusRepo.GetByIdAsync(candidacy.CurrentStatusId.Value, cancellationToken);
        if (currentStatus == null || currentStatus.IsFinal)
            return false;

        var targetStatus = await GetStatusByCodeAsync(candidacy.OrgUnitId, targetStatusCode, cancellationToken);
        if (targetStatus == null)
            return false;

        var transitions = await _transitionRepo.FindAsync(
            t => t.OrgUnitId == candidacy.OrgUnitId && t.FromStatusId == candidacy.CurrentStatusId.Value,
            cancellationToken);

        return transitions.Any(t => t.ToStatusId == targetStatus.Id);
    }

    public async Task<StatusTransitionResult> ExecuteTransitionAsync(
        int candidacyId, string targetStatusCode, string? reason, int userId, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepo.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        if (!candidacy.IsActive)
            return StatusTransitionResult.NotAllowed("מועמדות זו אינה פעילה");

        if (candidacy.CurrentStatusId == null)
            return StatusTransitionResult.NotAllowed("למועמדות זו לא הוגדר סטטוס נוכחי");

        var currentStatus = await _statusRepo.GetByIdAsync(candidacy.CurrentStatusId.Value, cancellationToken);
        if (currentStatus != null && currentStatus.IsFinal)
            return StatusTransitionResult.NotAllowed("מועמדות בסטטוס סופי אינה ניתנת לשינוי");

        var targetStatus = await GetStatusByCodeAsync(candidacy.OrgUnitId, targetStatusCode, cancellationToken);
        if (targetStatus == null)
            return StatusTransitionResult.Failed($"סטטוס '{targetStatusCode}' לא נמצא ביחידה הארגונית");

        var transitions = await _transitionRepo.FindAsync(
            t => t.OrgUnitId == candidacy.OrgUnitId && t.FromStatusId == candidacy.CurrentStatusId.Value,
            cancellationToken);

        var transition = transitions.FirstOrDefault(t => t.ToStatusId == targetStatus.Id);
        if (transition == null)
            return StatusTransitionResult.NotAllowed();

        if (transition.RequiresReason && string.IsNullOrWhiteSpace(reason))
            return StatusTransitionResult.Failed("מעבר סטטוס זה דורש ציון סיבה");

        // Record history
        var history = new CandidacyStatusHistory
        {
            CandidacyId = candidacyId,
            FromStatusId = candidacy.CurrentStatusId,
            ToStatusId = targetStatus.Id,
            Reason = reason,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow
        };

        var fromStatusCode = currentStatus?.Code ?? string.Empty;

        candidacy.CurrentStatusId = targetStatus.Id;

        if (targetStatus.IsFinal)
            candidacy.IsActive = false;

        await _candidacyRepo.UpdateAsync(candidacy, cancellationToken);
        await _historyRepo.AddAsync(history, cancellationToken);

        // Publish domain event
        await _mediator.Publish(
            new CandidacyStatusChangedEvent(candidacyId, candidacy.OrgUnitId, fromStatusCode, targetStatusCode, userId),
            cancellationToken);

        return StatusTransitionResult.Success(targetStatusCode);
    }

    public async Task<IEnumerable<string>> GetAllowedTransitionsAsync(int candidacyId, CancellationToken cancellationToken = default)
    {
        var candidacy = await _candidacyRepo.GetByIdAsync(candidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), candidacyId);

        if (!candidacy.IsActive || candidacy.CurrentStatusId == null)
            return Enumerable.Empty<string>();

        var currentStatus = await _statusRepo.GetByIdAsync(candidacy.CurrentStatusId.Value, cancellationToken);
        if (currentStatus == null || currentStatus.IsFinal)
            return Enumerable.Empty<string>();

        var transitions = await _transitionRepo.FindAsync(
            t => t.OrgUnitId == candidacy.OrgUnitId && t.FromStatusId == candidacy.CurrentStatusId.Value,
            cancellationToken);

        var targetStatusIds = transitions.Select(t => t.ToStatusId).ToList();
        var statuses = await _statusRepo.FindAsync(
            s => targetStatusIds.Contains(s.Id),
            cancellationToken);

        return statuses.Select(s => s.Code);
    }

    public async Task<WorkflowDefinitionDto?> GetWorkflowDefinitionAsync(int orgUnitId, CancellationToken cancellationToken = default)
    {
        var workflows = await _workflowRepo.FindAsync(
            w => w.OrgUnitId == orgUnitId && w.IsActive,
            cancellationToken);

        var workflow = workflows.OrderByDescending(w => w.Version).FirstOrDefault();
        if (workflow == null)
            return null;

        return new WorkflowDefinitionDto(
            workflow.Id, workflow.OrgUnitId, workflow.Name,
            workflow.ExamStepEnabled, workflow.InterviewStepEnabled,
            workflow.CommitteeStepEnabled, workflow.ThresholdCheckEnabled,
            workflow.StepOrder, workflow.Version, workflow.IsActive, workflow.CreatedAt);
    }

    private async Task<StatusDefinition?> GetStatusByCodeAsync(int orgUnitId, string code, CancellationToken cancellationToken)
    {
        var statuses = await _statusRepo.FindAsync(
            s => s.OrgUnitId == orgUnitId && s.Code == code,
            cancellationToken);
        return statuses.FirstOrDefault();
    }
}
