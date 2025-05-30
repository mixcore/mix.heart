﻿using Mix.Heart.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static Dictionary<string, string> escapeMapping = new Dictionary<string, string>()
        {
            {"\"", @"\\\"""},
            {"\\\\", @"\\"},
            {"\a", @"\a"},
            {"\b", @"\b"},
            {"\f", @"\f"},
            {"\n", @"\n"},
            {"\r", @"\r"},
            {"\t", @"\t"},
            {"\v", @"\v"},
            {"\0", @"\0"},
        };

        private static Regex escapeRegex = new Regex(string.Join("|", escapeMapping.Keys.ToArray()));

        public static string Escape(this string s)
        {
            return escapeRegex.Replace(s, EscapeMatchEval);
        }

        private static string EscapeMatchEval(Match m)
        {
            if (escapeMapping.ContainsKey(m.Value))
            {
                return escapeMapping[m.Value];
            }
            return escapeMapping[Regex.Escape(m.Value)];
        }

        public static string ToHyphenCase(this string source, char? replaceChar, bool isLower = true, bool isUpper = false)
        {
            source = char.ToLowerInvariant(source[0]) + source[1..];
            return Regex.Replace(source, @"[A-Z]", delegate (Match match)
            {
                string v = match.ToString();
                var c = isLower ? char.ToLower(v[0])
                            : isUpper ? char.ToLower(v[0]) : v[0];
                return $"{replaceChar}{c}{v[1..]}";
            });
        }

        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str[1..];
            }
            return str;
        }

        public static string ToTitleCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToUpperInvariant(str[0]) + str[1..];
            }
            return str;
        }

        public static string ToColumnName(this string str, bool isTitleCase = true)
        {
            return !string.IsNullOrEmpty(str)
                ? isTitleCase
                    ? str.ToHyphenCase(null, false, true)
                    : str.ToSEOString('_')
                : str;
        }

        public static string ToSEOString(this string str, char replaceChar = '-')
        {
            return !string.IsNullOrEmpty(str)
                ? WhiteSpaceToHyphen(ConvertToUnSign(DeleteSpecialCharaters(str)), replaceChar)
                : str;
        }

        public static byte[] ToByteArray(this string plainText)
        {
            return Encoding.UTF8.GetBytes(plainText);
        }

        public static bool IsJsonString(this string jsonString)
        {
            return !string.IsNullOrEmpty(jsonString) && jsonString.Trim().StartsWith("{") && jsonString.Trim().EndsWith("}");
        }

        public static bool IsJArrayString(this string jarrayString)
        {
            return !string.IsNullOrEmpty(jarrayString) && jarrayString.Trim().StartsWith("[") && jarrayString.Trim().EndsWith("]");
        }

        public static bool IsBase64(this string base64String)
        {
            base64String = base64String?.IndexOf(',') >= 0
                               ? base64String.Split(',')[1]
                               : base64String;
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0 ||
                base64String.Contains(' ') || base64String.Contains('\t') ||
                base64String.Contains('\r') || base64String.Contains('\n'))
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

        public static string ToBase64Stream(this string base64String)
        {
            return base64String?.IndexOf(',') >= 0
                               ? base64String.Split(',')[1]
                               : base64String;
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
            return AesEncryptionHelper.EncryptString(text, key);
        }

        public static string Decrypt(this string cipherText, string key)
        {
            return AesEncryptionHelper.DecryptString(cipherText, key);
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

            Regex regex = new(@"\p{IsCombiningDiacriticalMarks}+");

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
