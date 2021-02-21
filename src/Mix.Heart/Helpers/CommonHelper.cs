// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Mix.Domain.Core.ViewModels;
using Mix.Services;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static Mix.Heart.Domain.Constants.Common;

namespace Mix.Common.Helper
{
    /// <summary>
    /// Common helper
    /// </summary>
    public static class CommonHelper
    {
        private static volatile JObject webConfigInstance;
        private static readonly object syncRoot = new Object();

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

        public static JObject WebConfigInstance {
            get {
                if (webConfigInstance == null)
                {
                    lock (syncRoot)
                    {
                        if (webConfigInstance == null)
                            webConfigInstance = LoadWebConfig();
                    }
                }
                return webConfigInstance;
            }
            set {
                webConfigInstance = value;
            }
        }

        private static JObject LoadWebConfig()
        {
            // Load configurations from appSettings.json
            var settings = MixFileRepository.Instance.GetFile("appSettings.json", string.Empty, true);

            if (string.IsNullOrEmpty(settings.Content))
            {
                settings = MixFileRepository.Instance.GetFile("appSettings.json", string.Empty, true, "{}");
            }
            string content = string.IsNullOrWhiteSpace(settings.Content) ? "{}" : settings.Content;
            return JObject.Parse(content);
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
            return string.Format(strFormat, subPaths);
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
                string fileData = strBase64.Substring(strBase64.IndexOf(',') + 1);
                byte[] bytes = Convert.FromBase64String(fileData);
                return SaveFileBytes(folder, filename, bytes);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the file base64.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="strBase64">The string base64.</param>
        /// <returns></returns>
        public static bool SaveFileBytes(string folder, string filename, byte[] bytes)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)
            try
            {
                folder = GetFullPath(new string[]
                {
                    "wwwroot",
                    folder
                });
                string fullPath = GetFullPath(new string[]
                {
                    folder,
                    filename
                });
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
        public static void WriteBase64ToFile(string fullPath, string strBase64)
        {
            string fileData = strBase64.Substring(strBase64.IndexOf(',') + 1);
            byte[] bytes = Convert.FromBase64String(fileData);
            WriteBytesToFile(fullPath, bytes);
        }

        /// <summary>
        /// Writes the bytes to file.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="strBase64">The string base64.</param>
        public static void WriteBytesToFile(string fullPath, byte[] bytes)
        {
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

        public static RepositoryResponse<FileViewModel> ExportToExcel<T>(List<T> lstData, string sheetName
            , string folderPath, string fileName
            , List<string> headers = null)
        {
            var result = new RepositoryResponse<FileViewModel>()
            {
                Data = new FileViewModel()
                {
                    FileFolder = folderPath,
                    Filename = fileName + "-" + DateTime.Now.ToString("yyyyMMdd"),
                    Extension = ".xlsx"
                }
            };
            try
            {
                if (lstData.Count > 0)
                {
                    var filenameE = $"{result.Data.Filename}{result.Data.Extension}";

                    // create new data table
                    var dtable = new DataTable();

                    if (headers == null)
                    {
                        // get first item
                        var listColumn = lstData[0].GetType().GetProperties();

                        // add column name to table
                        foreach (var item in listColumn)
                        {
                            dtable.Columns.Add(item.Name, typeof(string));
                        }
                    }
                    else
                    {
                        foreach (var item in headers)
                        {
                            dtable.Columns.Add(item, typeof(string));
                        }
                    }

                    // Row value
                    foreach (var a in lstData)
                    {
                        var r = dtable.NewRow();
                        if (headers == null)
                        {
                            foreach (var prop in a.GetType().GetProperties())
                            {
                                r[prop.Name] = prop.GetValue(a, null);
                            }
                        }
                        else
                        {
                            var props = a.GetType().GetProperties();
                            for (int i = 0; i < headers.Count; i++)
                            {
                                r[i] = props[i].GetValue(a, null);
                            }
                        }

                        dtable.Rows.Add(r);
                    }

                    // Save Excel file
                    using (var pck = new ExcelPackage())
                    {
                        string SheetName = sheetName != string.Empty ? sheetName : "Report";
                        var wsDt = pck.Workbook.Worksheets.Add(SheetName);
                        wsDt.Cells["A1"].LoadFromDataTable(dtable, true, TableStyles.None);
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();

                        SaveFileBytes(folderPath, filenameE, pck.GetAsByteArray());
                        result.IsSucceed = true;

                        return result;
                    }
                }
                else
                {
                    result.Errors.Add("Can not export data of empty list");
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public static RepositoryResponse<FileViewModel> ExportJObjectToExcel(List<JObject> lstData, string sheetName
            , string folderPath, string fileName
            , List<string> headers = null)
        {
            var result = new RepositoryResponse<FileViewModel>()
            {
                Data = new FileViewModel()
                {
                    FileFolder = folderPath,
                    Filename = fileName + "-" + DateTime.Now.ToString("yyyyMMdd"),
                    Extension = ".xlsx"
                }
            };
            try
            {
                if (lstData.Count > 0)
                {
                    var filenameE = $"{result.Data.Filename}{result.Data.Extension}";

                    // create new data table
                    var dtable = new DataTable();

                    if (headers == null)
                    {
                        // get first item
                        var listColumn = lstData[0].Properties();

                        // add column name to table
                        foreach (var item in listColumn)
                        {
                            dtable.Columns.Add(item.Name, typeof(string));
                        }
                    }
                    else
                    {
                        foreach (var item in headers)
                        {
                            dtable.Columns.Add(item, typeof(string));
                        }
                    }

                    // Row value
                    foreach (var a in lstData)
                    {
                        var r = dtable.NewRow();
                        foreach (var prop in a.Properties())
                        {
                            r[prop.Name] = a[prop.Name]["value"];
                        }
                        dtable.Rows.Add(r);
                    }

                    // Save Excel file
                    using (var pck = new ExcelPackage())
                    {
                        string SheetName = sheetName != string.Empty ? sheetName : "Report";
                        var wsDt = pck.Workbook.Worksheets.Add(SheetName);
                        wsDt.Cells["A1"].LoadFromDataTable(dtable, true, TableStyles.None);
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();

                        SaveFileBytes(folderPath, filenameE, pck.GetAsByteArray());
                        result.IsSucceed = true;

                        return result;
                    }
                }
                else
                {
                    result.Errors.Add("Can not export data of empty list");
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public static RepositoryResponse<FileViewModel> ExportAttributeToExcel(List<JObject> lstData, string sheetName
           , string folderPath, string fileName
           , List<string> headers = null)
        {
            var result = new RepositoryResponse<FileViewModel>()
            {
                Data = new FileViewModel()
                {
                    FileFolder = folderPath,
                    Filename = fileName + "-" + DateTime.Now.ToString("yyyyMMdd"),
                    Extension = ".xlsx"
                }
            };
            try
            {
                if (lstData.Count > 0)
                {
                    var filenameE = $"{result.Data.Filename}{result.Data.Extension}";

                    // create new data table
                    var dtable = new DataTable();

                    if (headers == null)
                    {
                        // get first item
                        var listColumn = lstData[0].Properties();

                        // add column name to table
                        foreach (var item in listColumn)
                        {
                            dtable.Columns.Add(item.Name, typeof(string));
                        }
                    }
                    else
                    {
                        foreach (var item in headers)
                        {
                            dtable.Columns.Add(item, typeof(string));
                        }
                    }

                    // Row value
                    foreach (var a in lstData)
                    {
                        var r = dtable.NewRow();
                        foreach (var prop in a.Properties())
                        {
                            r[prop.Name] = a[prop.Name].Value<string>();
                        }
                        dtable.Rows.Add(r);
                    }

                    // Save Excel file
                    using (var pck = new ExcelPackage())
                    {
                        string SheetName = sheetName != string.Empty ? sheetName : "Report";
                        var wsDt = pck.Workbook.Worksheets.Add(SheetName);
                        wsDt.Cells["A1"].LoadFromDataTable(dtable, true, TableStyles.None);
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();

                        SaveFileBytes(folderPath, filenameE, pck.GetAsByteArray());
                        result.IsSucceed = true;
                        return result;
                    }
                }
                else
                {
                    result.Errors.Add("Can not export data of empty list");
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public static T GetWebConfig<T>(string name)
        {
            if (WebConfigInstance[WebConfiguration.MixConfigurations] != null)
            {
                var result = WebConfigInstance[WebConfiguration.MixConfigurations][name];
                return result != null ? result.Value<T>() : default(T);
            }
            else
            {
                var result = WebConfigInstance[name];
                return result != null ? result.Value<T>() : default(T);
            }
        }
        
        public static T GetWebEnumConfig<T>(string name)
        {
            if (WebConfigInstance[WebConfiguration.MixConfigurations] != null)
            {
                Enum.TryParse(typeof(T), WebConfigInstance[WebConfiguration.MixConfigurations][name]?.Value<string>(), true, out object result);
                return result != null ? (T)result : default;
            }
            else
            {
                Enum.TryParse(typeof(T), WebConfigInstance[name]?.Value<string>(), true, out object result);
                return result != null ? (T)result : default;
            }
        }

        public static List<object> ParseEnumToObject(Type enumType)
        {
            List<object> result = new List<object>();
            var values = Enum.GetValues(enumType);
            foreach (var item in values)
            {
                result.Add(new { name = Enum.GetName(enumType, item), value = Enum.ToObject(enumType, item) });
            }
            return result;
        }
    }
}