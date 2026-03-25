using CandidacyManagement.Domain.Entities;

namespace CandidacyManagement.Application.ExternalSubmissions;

/// <summary>
/// שירות תיעוד קריאות API חיצוני
/// </summary>
public interface IApiCallLogService
{
    /// <summary>
    /// תיעוד קריאת API
    /// </summary>
    Task LogAsync(ApiCallLog log, CancellationToken cancellationToken = default);

    /// <summary>
    /// שליפת יומן קריאות לפי מזהה מערכת חיצונית
    /// </summary>
    Task<IEnumerable<ApiCallLog>> GetBySystemIdAsync(string externalSystemId, CancellationToken cancellationToken = default);
}
