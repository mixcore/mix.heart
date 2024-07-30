using System;
using System.Security.Cryptography;
using System.Text;

namespace Mix.Heart.Helpers
{
    public class AesEncryptionHelper
    {
        public static string EncryptString(string text, string iCompleteEncodedKey)
        {
            string[] keyStrings =
                Encoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey))
                    .Split(',');

            var iv = Convert.FromBase64String(keyStrings[0]);
            var key = Convert.FromBase64String(keyStrings[1]);
            return EncryptString(text, key, iv);
        }

        public static string DecryptString(string cipherText, string iCompleteEncodedKey)
        {
            string[] keyStrings =
                Encoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey))
                    .Split(',');
            var iv = Convert.FromBase64String(keyStrings[0]);
            var key = Convert.FromBase64String(keyStrings[1]);
            return DecryptString(cipherText, key, iv);
        }

        public static string EncryptString(string text, string keyString, string ivString)
        {
            var iv = Encoding.UTF8.GetBytes(ivString);
            var key = Encoding.UTF8.GetBytes(keyString);
            return EncryptString(text, key, iv);
        }

        public static string DecryptString(string cipherText, string keyString, string ivString)
        {
            var iv = Encoding.UTF8.GetBytes(ivString);
            var key = Encoding.UTF8.GetBytes(keyString);
            return DecryptString(cipherText, key, iv);
        }

        private static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using var aesAlg = Aes.Create();
            aesAlg.BlockSize = 128;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            using var encryptor = aesAlg.CreateEncryptor(key, iv);
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherText =
                encryptor.TransformFinalBlock(bytes, 0, plainText.Length);
            return Convert.ToBase64String(cipherText);
        }

        private static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            using var decryptor = aesAlg.CreateDecryptor(key, iv);
            byte[] encryptedBytes = Convert.FromBase64CharArray(
                cipherText.ToCharArray(), 0, cipherText.Length);
            return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(
                encryptedBytes, 0, encryptedBytes.Length));
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
    }
}
