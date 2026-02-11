using System;
using System.Security.Cryptography;
using System.Text;

namespace LinksAndMore.Services;

public interface ISecurityService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class SecurityService : ISecurityService
{
    // Entropy to add more security to the protection (can be anything)
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("LinksAndMore-Entropy-Sec");

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            if (cipherBytes.Length == 0) return string.Empty;

            byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception)
        {
            // Decryption failed (might be a different user or machine)
            return "[Error: Decryption Failed]";
        }
    }
}
