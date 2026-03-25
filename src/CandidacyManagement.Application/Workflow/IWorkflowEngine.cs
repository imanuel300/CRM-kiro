namespace CandidacyManagement.Application.Workflow;

/// <summary>
/// מנוע תהליך מיון - מנהל מעברי סטטוס בהתאם להגדרות State Machine
/// </summary>
public interface IWorkflowEngine
{
    Task<bool> CanTransitionAsync(int candidacyId, string targetStatusCode, CancellationToken cancellationToken = default);
    Task<StatusTransitionResult> ExecuteTransitionAsync(int candidacyId, string targetStatusCode, string? reason, int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetAllowedTransitionsAsync(int candidacyId, CancellationToken cancellationToken = default);
    Task<WorkflowDefinitionDto?> GetWorkflowDefinitionAsync(int orgUnitId, CancellationToken cancellationToken = default);
}
