namespace CandidacyManagement.Application.ThresholdChecks;

/// <summary>
/// שירות בדיקת תנאי סף - בדיקה אוטומטית וידנית של עמידה בתנאי סף
/// </summary>
public interface IThresholdCheckService
{
    /// <summary>בדיקת כל תנאי הסף למועמדות</summary>
    Task<CheckAllResultDto> CheckAllAsync(int candidacyId, CancellationToken cancellationToken = default);

    /// <summary>בדיקת תנאי סף בודד</summary>
    Task<ThresholdCheckResultDto> CheckSingleAsync(int candidacyId, int conditionId, CancellationToken cancellationToken = default);

    /// <summary>בדיקה ידנית של תנאי סף</summary>
    Task<ThresholdCheckResultDto> ManualCheckAsync(ManualCheckCommand command, CancellationToken cancellationToken = default);

    /// <summary>שליפת תוצאות בדיקת סף למועמדות</summary>
    Task<IEnumerable<ThresholdCheckResultDto>> GetResultsAsync(int candidacyId, CancellationToken cancellationToken = default);
}
