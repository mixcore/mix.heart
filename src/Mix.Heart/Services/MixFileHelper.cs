using Microsoft.AspNetCore.Http;
using Mix.Heart.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Mix.Heart.Services
{
    public static class MixFileHelper
    {
        public static string CurrentDirectory { get; set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="MixFileRepository"/> class from being created.
        /// </summary>
        static MixFileHelper()
        {
            CurrentDirectory = Environment.CurrentDirectory;
        }

        #region Read Files
        public static FileModel GetFile(
            string name,
            string ext,
            string FileFolder,
            bool isCreate = false,
            string defaultContent = null)
        {
            FileModel result = null;

            string fullPath = $"{CurrentDirectory}/{FileFolder}/{name}{ext}";

            FileInfo fileinfo = new FileInfo(fullPath);

            if (fileinfo.Exists)
            {
                try
                {
                    using (var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader s = new StreamReader(stream))
                        {
                            result = new FileModel()
                            {
                                FileFolder = FileFolder,
                                Filename = name,
                                Extension = ext,
                                Content = s.ReadToEnd()
                            };
                        }
                    }
                }
                catch
                {
                    // File invalid
                }
            }
            else if (isCreate)
            {
                CreateFolderIfNotExist(FileFolder);
                fileinfo.Create();
                result = new FileModel()
                {
                    FileFolder = FileFolder,
                    Filename = name,
                    Extension = ext,
                    Content = defaultContent
                };
                SaveFile(result);
            }

            return result ?? new FileModel() { FileFolder = FileFolder };
        }

        public static FileModel GetFileByFullName(
           string fullName,
           bool isCreate = false,
           string defaultContent = null)
        {
            FileModel result = null;

            string fullPath = $"{CurrentDirectory}/{fullName}";

            FileInfo fileinfo = new FileInfo(fullPath);
            string folder = fullName[..fullName.LastIndexOf('/')];
            string filename = fullName[(fullName.LastIndexOf('/') + 1)..];
            string name = filename[..filename.LastIndexOf('.')];
            string ext = filename[filename.LastIndexOf('.')..];
            if (fileinfo.Exists)
            {
                try
                {
                    using (var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader s = new StreamReader(stream))
                        {
                            result = new FileModel()
                            {
                                FileFolder = folder,
                                Filename = name,
                                Extension = ext,
                                Content = s.ReadToEnd()
                            };
                        }
                    }
                }
                catch
                {
                    // File invalid
                }
            }
            else if (isCreate)
            {
                result = new FileModel()
                {
                    FileFolder = folder,
                    Filename = name,
                    Extension = ext,
                    Content = defaultContent
                };
                SaveFile(result);
            }

            return result ?? new FileModel() { FileFolder = folder, Filename = name, Extension = ext };
        }
        #endregion

        #region Create / Delete File or Folder

        #region File

        public static string SaveFile(FileModel file)
        {
            try
            {
                string fileName = $"{file.Filename}{file.Extension}";
                if (!string.IsNullOrEmpty(file.Filename))
                {
                    CreateFolderIfNotExist(file.FileFolder);

                    string filePath = fileName;
                    if (!string.IsNullOrEmpty(file.FileFolder))
                    {
                        filePath = $"{file.FileFolder}/{filePath}";
                    }
                    if (File.Exists(filePath))
                    {
                        DeleteFile(filePath);
                    }
                    if (!string.IsNullOrEmpty(file.Content))
                    {
                        using (var writer = File.CreateText(filePath))
                        {
                            writer.WriteLine(file.Content); //or .Write(), if you wish
                            writer.Dispose();
                            return fileName;
                        }
                    }
                    else if (file.FileStream != null)
                    {
                        string base64 = file.FileStream.Split(',')[1];
                        byte[] bytes = Convert.FromBase64String(base64);
                        using (var writer = File.Create(filePath))
                        {
                            writer.Write(bytes, 0, bytes.Length);
                            return fileName;
                        }
                    }
                    else
                    {
                        File.CreateText(filePath);
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

        public static string SaveFile(IFormFile file, string fullPath)
        {
            try
            {
                if (file.Length > 0)
                {
                    CreateFolderIfNotExist(fullPath);
                    string fileName = file.FileName;
                    string fullPath2 = $"{fullPath}/{fileName}";
                    if (File.Exists(fullPath2))
                    {
                        DeleteFile(fullPath2);
                    }

                    using (FileStream target = new FileStream(fullPath2, FileMode.Create))
                    {
                        file.CopyTo(target);
                    }

                    return fileName;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                bool result = false;
                File.Delete(filePath);
                result = true;
                return result;
            }
            return true;
        }

        public static List<FileModel> GetTopFiles(string folder)
        {
            List<FileModel> result = new List<FileModel>();
            if (Directory.Exists(folder))
            {
                DirectoryInfo path = new DirectoryInfo(folder);
                string folderName = path.Name;

                var Files = path.GetFiles();
                foreach (var file in Files.OrderByDescending(f => f.CreationTimeUtc))
                {
                    result.Add(new FileModel()
                    {
                        FolderName = folderName,
                        FileFolder = folder,

                        Filename = file.Name.Substring(0, file.Name.LastIndexOf('.') >= 0 ? file.Name.LastIndexOf('.') : 0),
                        Extension = file.Extension,
                        //Content = s.ReadToEnd()
                    });
                }
            }
            return result;
        }
        #endregion

        #region Folder
        public static List<string> GetTopDirectories(string folder)
        {
            List<string> result = new List<string>();
            if (Directory.Exists(folder))
            {
                foreach (string dirPath in Directory.GetDirectories(folder, "*",
                    SearchOption.TopDirectoryOnly))
                {
                    DirectoryInfo path = new DirectoryInfo(dirPath);
                    result.Add(path.Name);
                }
            }
            return result;
        }
        public static bool CopyFolder(string srcPath, string desPath)
        {
            if (srcPath.ToLower() != desPath.ToLower() && Directory.Exists(srcPath))
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(srcPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(srcPath, desPath));
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(srcPath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(srcPath, desPath), true);
                }

                return true;
            }
            return true;
        }

        public static bool DeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                return true;
            }
            return false;
        }

        public static bool EmptyFolder(string folderPath)
        {
            DeleteFolder(folderPath);
            CreateFolderIfNotExist(folderPath);
            return true;
        }

        public static void CreateFolderIfNotExist(string fullPath)
        {
            if (!string.IsNullOrEmpty(fullPath) && !Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public static List<string> GetTopFolders(string folder)
        {
            List<string> result = new List<string>();
            if (Directory.Exists(folder))
            {
                foreach (string dirPath in Directory.GetDirectories(folder, "*",
                    SearchOption.TopDirectoryOnly))
                {
                    DirectoryInfo path = new DirectoryInfo(dirPath);
                    result.Add(path.Name);
                }
            }
            return result;
        }

        public static string ZipFolder(string tmpPath, string outputPath, string fileName)
        {
            try
            {
                //string tmpPath = $"wwwroot/Exports/temp/{fileName}-{DateTime.UtcNow.ToShortDateString()}";
                string outputFile = $"{outputPath}/{fileName}.zip";
                string outputFilePath = $"{outputPath}/{fileName}.zip";

                if (Directory.Exists(tmpPath))
                {
                    //CopyDirectory(srcFolder, tmpPath);
                    if (File.Exists(outputFile))
                    {
                        File.Delete(outputFile);
                    }
                    ZipFile.CreateFromDirectory(tmpPath, outputFile);
                    DeleteFolder(tmpPath);
                    return outputFilePath;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static void UnZipFile(string filePath, string webFolder)
        {
            try
            {
                ZipFile.ExtractToDirectory(filePath, webFolder, true);
            }
            catch
            {
                //throw;
            }
        }
        #endregion

        #endregion
    }
}
