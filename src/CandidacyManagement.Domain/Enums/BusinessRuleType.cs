namespace CandidacyManagement.Domain.Enums;

/// <summary>
/// סוגי כללים עסקיים הנתמכים במנוע הכללים
/// </summary>
public enum BusinessRuleType
{
    DuplicatePrevention,
    ThresholdCheck,
    ScoreCalculation,
    AutoStatusTransition,
    DocumentValidation,
    EligibilityCheck
}
