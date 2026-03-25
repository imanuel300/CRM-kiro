namespace CandidacyManagement.Application.ExternalSubmissions;

/// <summary>
/// שירות קליטת מועמדויות ממערכת הגשה חיצונית
/// </summary>
public interface IExternalSubmissionService
{
    /// <summary>
    /// קליטת מועמדות חדשה ממערכת חיצונית
    /// </summary>
    Task<SubmissionResult> SubmitAsync(ExternalSubmissionCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// ולידציה של נתוני מועמדות לפני קליטה
    /// </summary>
    Task<SubmissionValidationResult> ValidateAsync(ExternalSubmissionCommand command, CancellationToken cancellationToken = default);
}
