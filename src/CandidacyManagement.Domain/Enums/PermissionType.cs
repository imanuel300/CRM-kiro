namespace CandidacyManagement.Domain.Enums;

/// <summary>
/// סוגי הרשאות במערכת - מגדיר את הפעולות המותרות לכל תפקיד
/// </summary>
public enum PermissionType
{
    /// <summary>צפייה בנתונים</summary>
    View,
    /// <summary>עריכת נתונים</summary>
    Edit,
    /// <summary>יצירת רשומות חדשות</summary>
    Create,
    /// <summary>מחיקת רשומות</summary>
    Delete,
    /// <summary>שינוי סטטוס מועמדות</summary>
    ChangeStatus,
    /// <summary>שליחת דיוור והודעות</summary>
    SendNotification
}
