namespace CandidacyManagement.Application.Workflow;

/// <summary>
/// שירות הגדרת תהליכי מיון - קונפיגורציה ברמת יחידה ארגונית
/// </summary>
public interface IWorkflowConfigService
{
    Task<WorkflowDefinitionDto> ConfigureWorkflowAsync(ConfigureWorkflowCommand command, CancellationToken cancellationToken = default);
    Task<IEnumerable<StatusDefinitionDto>> ConfigureStatusesAsync(ConfigureStatusesCommand command, CancellationToken cancellationToken = default);
    Task<IEnumerable<StatusTransitionDto>> ConfigureTransitionsAsync(ConfigureTransitionsCommand command, CancellationToken cancellationToken = default);
}
