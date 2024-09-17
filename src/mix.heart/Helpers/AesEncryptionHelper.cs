using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mix.Heart.Helpers
{
public class AesEncryptionHelper
{
    public static string EncryptString(string text, string iCompleteEncodedKey, Encoding encoding = default)
    {
        if (text is null)
        {
            return default;
        }

        string[] keyStrings =
            Encoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey))
            .Split(',');

        var iv = Convert.FromBase64String(keyStrings[0]);
        var key = Convert.FromBase64String(keyStrings[1]);
        return EncryptString(text, key, iv, encoding ?? Encoding.UTF8);
    }

    public static string DecryptString(string cipherText, string iCompleteEncodedKey, Encoding encoding = default)
    {
        string[] keyStrings =
            Encoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey))
            .Split(',');
        var iv = Convert.FromBase64String(keyStrings[0]);
        var key = Convert.FromBase64String(keyStrings[1]);
        return DecryptString(cipherText, key, iv, encoding ?? Encoding.UTF8);
    }

    public static string EncryptString(string text, string keyString, string ivString, Encoding encoding = default)
    {
        var iv = Encoding.UTF8.GetBytes(ivString);
        var key = Encoding.UTF8.GetBytes(keyString);
        return EncryptString(text, key, iv, encoding ?? Encoding.UTF8);
    }

    public static string DecryptString(string cipherText, string keyString, string ivString,
                                       Encoding encoding = default)
    {
        var iv = Encoding.UTF8.GetBytes(ivString);
        var key = Encoding.UTF8.GetBytes(keyString);
        return DecryptString(cipherText, key, iv, encoding ?? Encoding.UTF8);
    }

    private static string EncryptString(string plainText, byte[] key, byte[] iv, Encoding encoding)
    {
        using var aesAlg = Aes.Create();
        aesAlg.BlockSize = 128;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        using var encryptor = aesAlg.CreateEncryptor(key, iv);
        byte[] bytes = encoding.GetBytes(plainText);
        byte[] cipherText;
        using (var msEncrypt = new MemoryStream())
        {
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                cipherText = msEncrypt.ToArray();
            }
        }

        return Convert.ToBase64String(cipherText);
    }

    private static string DecryptString(string cipherText, byte[] key, byte[] iv, Encoding encoding)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        using var decryptor = aesAlg.CreateDecryptor(key, iv);

        using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        var plaintext = srDecrypt.ReadToEnd();

        return plaintext;
    }

    public static string GenerateCombinedKeys()
    {
        using var aesEncryption = Aes.Create();
        aesEncryption.BlockSize = 128;
        aesEncryption.Mode = CipherMode.CBC;
        aesEncryption.Padding = PaddingMode.PKCS7;
        aesEncryption.GenerateIV();
        string ivStr = Convert.ToBase64String(aesEncryption.IV);
        aesEncryption.GenerateKey();
        string keyStr = Convert.ToBase64String(aesEncryption.Key);
        string completeKey = ivStr + "," + keyStr;

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(completeKey));
    }

    public static bool IsEncrypted(string input, string iCompleteEncodedKey)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            var cipherBytes = Convert.FromBase64String(input);
        }
        catch (FormatException)
        {
            return false;
        }

        try
        {
            DecryptString(input, iCompleteEncodedKey);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
}