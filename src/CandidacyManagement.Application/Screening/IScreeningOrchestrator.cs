namespace CandidacyManagement.Application.Screening;

/// <summary>
/// מתזמר תהליך מיון - מחבר את כל שלבי המיון (מבחנים, ראיונות, ועדות, תנאי סף)
/// ומנהל מעבר אוטומטי בין שלבים בהתאם לתהליך המוגדר ליחידה הארגונית
/// </summary>
public interface IScreeningOrchestrator
{
    /// <summary>
    /// מעבד השלמת שלב מיון ומבצע מעבר אוטומטי לשלב הבא
    /// </summary>
    Task<ScreeningStageResult> ProcessStageCompletionAsync(
        int candidacyId, string completedStage, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// מתחיל תהליך מיון למועמדות חדשה - בודק תנאי סף ומעביר לשלב הראשון
    /// </summary>
    Task<ScreeningStageResult> InitiateCandidacyScreeningAsync(
        int candidacyId, int userId, CancellationToken cancellationToken = default);
}

public record ScreeningStageResult(
    bool IsSuccess,
    string? CurrentStage,
    string? NextStage,
    string? ErrorMessage)
{
    public static ScreeningStageResult Success(string currentStage, string? nextStage) =>
        new(true, currentStage, nextStage, null);

    public static ScreeningStageResult Failed(string error) =>
        new(false, null, null, error);

    public static ScreeningStageResult ThresholdFailed(string stage) =>
        new(false, stage, null, "המועמד לא עמד בתנאי הסף");

    public static ScreeningStageResult NoWorkflowDefined() =>
        new(false, null, null, "לא הוגדר תהליך מיון ליחידה הארגונית");
}
