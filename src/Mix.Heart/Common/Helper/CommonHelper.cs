// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

namespace Mix.Common.Helper
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Common helper
    /// </summary>
    public static class CommonHelper
    {
        /// <summary>
        /// The base62chars
        /// </summary>
        private static readonly char[] Base62Chars =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
            .ToCharArray();

        /// <summary>
        /// The random
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <returns></returns>
        public static RSAParameters GenerateKey()
        {
            using (var key = new RSACryptoServiceProvider(2048))
            {
                return key.ExportParameters(true);
            }
        }

        /// <summary>
        /// Gets the base62.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GetBase62(int length)
        {
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append(Base62Chars[Random.Next(62)]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="subPaths">The sub paths.</param>
        /// <returns></returns>
        public static string GetFullPath(string[] subPaths)
        {
            string strFormat = string.Empty;
            for (int i = 0; i < subPaths.Length; i++)
            {
                // TODO: Use regular string literal instead of verbatim string literal => Remove @?
                strFormat += @"{" + i + "}" + (i < subPaths.Length - 1 ? "/" : string.Empty);
            }
            return string.Format(strFormat, subPaths).Replace("//", "/");
        }

        /// <summary>
        /// Gets the random name.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static string GetRandomName(string filename)
        {
            string ext = filename.Split('.')[1];
            return string.Format("{0}.{1}", Guid.NewGuid().ToString("N"), ext);
        }

        /// <summary>
        /// Gets the web response asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task<string> GetWebResponseAsync(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            using (WebResponse response = await webRequest.GetResponseAsync().ConfigureAwait(false))
            {
                using (Stream resStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Loads the image.
        /// </summary>
        /// <param name="strImage64">The string image64.</param>
        /// <returns></returns>
        public static Stream LoadImage(string strImage64)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)
            try
            {
                string imgData = strImage64.Substring(strImage64.IndexOf(',') + 1);
                byte[] imageBytes = Convert.FromBase64String(imgData);
                return new MemoryStream(imageBytes, 0, imageBytes.Length);
            }
            catch//(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Parses the name of the json property.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string ParseJsonPropertyName(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return Char.ToLower(input[0]) + input.Substring(1);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reads from file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static string ReadFromFile(string filename)
        {
            string s = "";
            try
            {
                FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(file);
                s = sr.ReadToEnd();
                sr.Dispose();
                file.Dispose();
            }
            catch
            {
                s = "";
            }
            return s;
        }

        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static bool RemoveFile(string filePath)
        {
            bool result = false;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Saves the file base64.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="strBase64">The string base64.</param>
        /// <returns></returns>
        public static bool SaveFileBase64(string folder, string filename, string strBase64)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)
            try
            {
                string fullPath = GetFullPath(new string[]
                {
                    folder,
                    filename
                });
                string fileData = strBase64.Substring(strBase64.IndexOf(',') + 1);
                byte[] bytes = Convert.FromBase64String(fileData);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                FileStream fs = new FileStream(fullPath, FileMode.Create);
                BinaryWriter w = new BinaryWriter(fs);
                try
                {
                    w.Write(bytes);
                }
                finally
                {
                    fs.Close();
                    w.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Uploads the file asynchronous.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task<string> UploadFileAsync(string fullPath, IFormFile file)
        {
            try
            {
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                if (file != null)
                {
                    //var fileName = ContentDispositionHeaderValue.Parse
                    //    (file.ContentDisposition).FileName.Trim('"');
                    string fileName = string.Format("{0}.{1}",
                        Guid.NewGuid().ToString("N"),
                        file.FileName.Split('.').Last());
                    using (var fileStream = new FileStream(Path.Combine(fullPath, fileName), FileMode.Create, FileAccess.ReadWrite))
                    {
                        await file.CopyToAsync(fileStream).ConfigureAwait(false);
                        return fileName;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes the bytes to file.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="strBase64">The string base64.</param>
        public static void WriteBytesToFile(string fullPath, string strBase64)
        {
            string fileData = strBase64.Substring(strBase64.IndexOf(',') + 1);
            byte[] bytes = Convert.FromBase64String(fileData);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            FileStream fs = new FileStream(fullPath, FileMode.Create);
            BinaryWriter w = new BinaryWriter(fs);
            try
            {
                w.Write(bytes);
            }
            finally
            {
                fs.Close();
                w.Close();
            }
        }
    }
}