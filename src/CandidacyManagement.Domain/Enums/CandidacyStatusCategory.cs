namespace CandidacyManagement.Domain.Enums;

/// <summary>
/// קטגוריות סטטוס מועמדות - מגדיר את הקטגוריות הכלליות של סטטוסים בתהליך המיון
/// </summary>
public enum CandidacyStatusCategory
{
    Submitted,
    InReview,
    Exam,
    Interview,
    Committee,
    Accepted,
    Rejected,
    Withdrawn
}
