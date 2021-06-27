using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Mix.Heart.Extensions
{
    public static class StringExtension
    {
        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        public static string ToHypenCase(this string source)
        {
            return Regex.Replace(source, @"[A-Z]", "-$1");
        }

        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }

        public static string ToTitleCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToUpperInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }

        public static string ToSEOString(this string str, char replaceChar = '-')
        {
            return !string.IsNullOrEmpty(str) 
                ? WhiteSpaceToHyphen(ConvertToUnSign(DeleteSpecialCharaters(str)), replaceChar) 
                : str;
        }

        public static bool IsBase64(this string base64String)
        {
            base64String = base64String?.IndexOf(',') >= 0
                               ? base64String.Split(',')[1]
                               : base64String;
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0 ||
                base64String.Contains(" ") || base64String.Contains("\t") ||
                base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Encrypt(this string text, string key)
        {
            var keybytes = Encoding.UTF8.GetBytes(key);
            var iv = Encoding.UTF8.GetBytes(key);
            var encrypted = EncryptStringToBytes(text, keybytes, iv);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(this string cipherText, string key)
        {
            var keybytes = Encoding.UTF8.GetBytes(key);
            var iv = Encoding.UTF8.GetBytes(key);
            var encrypted = Convert.FromBase64String(cipherText);
            return DecryptStringFromBytes(encrypted, keybytes, iv);
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] key,
                                                   byte[] iv)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            byte[] encrypted;
            // Create a RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor,
                                                           CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key,
                                                     byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                // Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor,
                                                               CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                catch
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;
        }

        public static string WhiteSpaceToHyphen(string str, char replaceChar = '-')
        {
            string pattern = " |–";
            MatchCollection matchs = Regex.Matches(str, pattern, RegexOptions.IgnoreCase);
            foreach (Match m in matchs)
            {
                str = str.Replace(m.Value[0], replaceChar);
            }
            replaceChar = '\'';
            pattern = "\"|“|”";
            matchs = Regex.Matches(str, pattern, RegexOptions.IgnoreCase);
            foreach (Match m in matchs)
            {
                str = str.Replace(m.Value[0], replaceChar);
            }
            return str.ToLower();
        }

        /// <summary>
        /// Converts to un sign.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string ConvertToUnSign(string text)
        {
            if (text != null)
            {
                for (int i = 33; i < 48; i++)
                {
                    text = text.Replace(((char)i).ToString(), "");
                }

                for (int i = 58; i < 65; i++)
                {
                    text = text.Replace(((char)i).ToString(), "");
                }

                for (int i = 91; i < 97; i++)
                {
                    text = text.Replace(((char)i).ToString(), "");
                }

                for (int i = 123; i < 127; i++)
                {
                    text = text.Replace(((char)i).ToString(), "");
                }
            }
            else
            {
                text = "";
            }

            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");

            string strFormD = text.Normalize(System.Text.NormalizationForm.FormD);

            return regex.Replace(strFormD, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string DeleteSpecialCharaters(string str)
        {
            const string replaceChar = "";
            string[] pattern = { ".", "/", "\\", "&", ":", "%" };

            foreach (string item in pattern)
            {
                str = str.Replace(item, replaceChar);
            }
            return str;
        }
    }
}
