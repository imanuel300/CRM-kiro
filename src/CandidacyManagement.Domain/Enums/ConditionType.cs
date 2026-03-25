namespace CandidacyManagement.Domain.Enums;

/// <summary>
/// סוג תנאי סף - מגדיר את סוג הבדיקה שתתבצע
/// </summary>
public enum ConditionType
{
    /// <summary>גיל מינימלי/מקסימלי</summary>
    Age,
    /// <summary>רמת השכלה נדרשת</summary>
    Education,
    /// <summary>ציון מינימלי</summary>
    Score,
    /// <summary>תנאי מותאם אישית - בדיקה ידנית</summary>
    Custom
}
