namespace CandidacyManagement.Domain.Enums;

/// <summary>
/// סוג אירוע מפעיל לשליחת הודעה אוטומטית
/// </summary>
public enum TriggerEventType
{
    /// <summary>שינוי סטטוס מועמדות</summary>
    StatusChange,

    /// <summary>זימון לראיון</summary>
    InterviewScheduled,

    /// <summary>זימון למבחן</summary>
    ExamScheduled,

    /// <summary>החלטת ועדה</summary>
    CommitteeDecision
}
