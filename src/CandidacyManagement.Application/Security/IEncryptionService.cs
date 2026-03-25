namespace CandidacyManagement.Application.Security;

/// <summary>
/// ממשק הצפנה ברמת עמודה (Column-Level Encryption) למידע אישי רגיש
/// דרישה: 18.1 - encryption at rest
/// </summary>
public interface IEncryptionService
{
    /// <summary>הצפנת ערך טקסט</summary>
    string Encrypt(string plainText);

    /// <summary>פענוח ערך מוצפן</summary>
    string Decrypt(string cipherText);
}
