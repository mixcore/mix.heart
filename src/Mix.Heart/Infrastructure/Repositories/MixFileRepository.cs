﻿// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0 license.
// See the LICENSE file in the project root for more information.

// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Mix.Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Newtonsoft.Json.Linq;
using Mix.Heart.Constants;
using Mix.Heart.Models;

namespace Mix.Infrastructure.Repositories
{
    public class MixFileRepository
    {
        public string CurrentDirectory { get; set; }


        /// <summary>
        /// The instance
        /// </summary>
        private static volatile MixFileRepository instance;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private static readonly object syncRoot = new Object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns></returns>
        public static MixFileRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MixFileRepository();
                    }
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="MixFileRepository"/> class from being created.
        /// </summary>
        private MixFileRepository()
        {
            CurrentDirectory = Environment.CurrentDirectory;
        }

        public FileViewModel GetFile(string FilePath, List<FileViewModel> Files, string FileFolder)
        {
            var result = Files.Find(v => !string.IsNullOrEmpty(FilePath) && v.Filename == FilePath.Replace(@"\", "/").Split('/')[1]);
            return result ?? new FileViewModel() { FileFolder = FileFolder };
        }

        public FileViewModel GetWebFile(string filename, string folder)
        {
            string fullPath = $"wwwroot/{folder}/{filename}";
            string folderPath = $"wwwroot/{folder}";
            FileInfo file = new FileInfo(fullPath);
            FileViewModel result = null;
            try
            {
                DirectoryInfo path = new DirectoryInfo(folderPath);
                using (StreamReader s = file.OpenText())
                {
                    result = new FileViewModel()
                    {
                        FolderName = path.Name,
                        FileFolder = folder,
                        Filename = file.Name.Substring(0, file.Name.LastIndexOf('.')),
                        Extension = file.Extension,
                        Content = s.ReadToEnd()
                    };
                }
            }
            catch
            {
                // File invalid
            }

            return result ?? new FileViewModel() { FileFolder = folder };
        }

        public bool DeleteWebFile(string filename, string folder)
        {
            string fullPath = MixCommonHelper.GetFullPath(new string[]
           {
                "wwwroot",
                folder,
                filename
           });

            if (File.Exists(fullPath))
            {
                MixCommonHelper.RemoveFile(fullPath);
            }
            return true;
        }

        public bool DeleteWebFile(string filePath)
        {
            string fullPath = MixCommonHelper.GetFullPath(new string[]
           {
                "wwwroot",
                filePath
           });

            if (File.Exists(fullPath))
            {
                MixCommonHelper.RemoveFile(fullPath);
            }
            return true;
        }

        public bool DeleteWebFolder(string folderPath)
        {
            string fullPath = MixCommonHelper.GetFullPath(new string[]
            {
                "wwwroot",
                folderPath
            });

            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            return true;
        }

        public FileViewModel GetUploadFile(string name, string ext, string fileFolder)
        {
            FileViewModel result = null;

            string folder = MixCommonHelper.GetFullPath(new string[] { fileFolder });
            string fullPath = string.Format(@"{0}/{1}.{2}", folder, name, ext);

            FileInfo file = new FileInfo(fullPath);

            try
            {
                using (StreamReader s = file.OpenText())
                {
                    result = new FileViewModel()
                    {
                        FileFolder = fileFolder,
                        Filename = file.Name.Substring(0, file.Name.LastIndexOf('.')),
                        Extension = file.Extension.Remove(0, 1),
                        Content = s.ReadToEnd()
                    };
                }
            }
            catch
            {
                // File invalid
            }
            return result ?? new FileViewModel() { FileFolder = fileFolder };
        }

        public FileViewModel GetFile(string name, string ext, string FileFolder, bool isCreate = false, string defaultContent = "")
        {
            FileViewModel result = null;

            string fullPath = Path.Combine(CurrentDirectory, FileFolder, string.Format("{0}{1}", name, ext));

            FileInfo fileinfo = new FileInfo(fullPath);

            if (fileinfo.Exists)
            {
                try
                {
                    using (var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader s = new StreamReader(stream))
                        {
                            result = new FileViewModel()
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
                CreateDirectoryIfNotExist(FileFolder);
                fileinfo.Create();
                result = new FileViewModel()
                {
                    FileFolder = FileFolder,
                    Filename = name,
                    Extension = ext,
                    Content = defaultContent
                };
                SaveFile(result);
            }

            return result ?? new FileViewModel() { FileFolder = FileFolder };
        }

        public FileViewModel GetFile(string fullname, string FileFolder, bool isCreate = false, string defaultContent = "")
        {
            var arr = fullname.Split('.');
            if (arr.Length >= 2)
            {
                return GetFile(fullname.Substring(0, fullname.LastIndexOf('.')), $".{arr[arr.Length - 1]}", FileFolder, isCreate, defaultContent);
            }
            else
            {
                return new FileViewModel() { FileFolder = FileFolder };
            }
        }

        public bool DeleteFile(string name, string extension, string FileFolder)
        {
            string folder = MixCommonHelper.GetFullPath(new string[] { FileFolder });
            string fullPath = string.Format(@"{0}/{1}{2}", folder, name, extension);

            if (File.Exists(fullPath))
            {
                MixCommonHelper.RemoveFile(fullPath);
            }
            return true;
        }

        public bool DeleteWebFile(string name, string extension, string FileFolder)
        {
            string fullPath = string.Format(@"{0}/{1}/{2}{3}", "wwwroot", FileFolder, name, extension);

            if (File.Exists(fullPath))
            {
                MixCommonHelper.RemoveFile(fullPath);
            }
            return true;
        }

        public bool DeleteFile(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                MixCommonHelper.RemoveFile(fullPath);
            }
            return true;
        }

        public bool DeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                return true;
            }
            return false;
        }

        public bool EmptyFolder(string folderPath)
        {
            DeleteFolder(folderPath);
            CreateDirectoryIfNotExist(folderPath);
            return true;
        }

        public bool CopyDirectory(string srcPath, string desPath)
        {
            if (srcPath.ToLower() != desPath.ToLower() && Directory.Exists(srcPath))
            {
                Directory.CreateDirectory(desPath);
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

        public bool CopyWebDirectory(string srcPath, string desPath)
        {
            if (srcPath != desPath)
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories($"wwwroot/{srcPath}", "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(srcPath, desPath));
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles($"wwwroot/{srcPath}", "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(srcPath, desPath), true);
                }

                return true;
            }
            return true;
        }

        public void CreateDirectoryIfNotExist(string fullPath)
        {
            if (!string.IsNullOrEmpty(fullPath) && !Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public List<FileViewModel> GetUploadFiles(string folder)
        {
            string fullPath = MixCommonHelper.GetFullPath(new string[] { folder });

            CreateDirectoryIfNotExist(fullPath);

            DirectoryInfo d = new DirectoryInfo(fullPath); //Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles();
            List<FileViewModel> result = new List<FileViewModel>();
            foreach (var file in Files.OrderByDescending(f => f.CreationTimeUtc))
            {
                using (StreamReader s = file.OpenText())
                {
                    result.Add(new FileViewModel()
                    {
                        FileFolder = folder,
                        Filename = file.Name.Substring(0, file.Name.LastIndexOf('.')),
                        Extension = file.Extension,
                        Content = s.ReadToEnd()
                    });
                }
            }
            return result;
        }

        public List<string> GetTopDirectories(string folder)
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

        public List<FileViewModel> GetTopFiles(string folder)
        {
            List<FileViewModel> result = new List<FileViewModel>();
            if (Directory.Exists(folder))
            {
                DirectoryInfo path = new DirectoryInfo(folder);
                string folderName = path.Name;

                var Files = path.GetFiles();
                foreach (var file in Files.OrderByDescending(f => f.CreationTimeUtc))
                {
                    result.Add(new FileViewModel()
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

        public List<FileViewModel> GetFilesWithContent(string fullPath)
        {
            CreateDirectoryIfNotExist(fullPath);

            //DirectoryInfo d = new DirectoryInfo(fullPath); //Assuming Test is your Folder
            FileInfo[] Files;
            List<FileViewModel> result = new List<FileViewModel>();
            foreach (string dirPath in Directory.GetDirectories(fullPath, "*",
                SearchOption.AllDirectories))
            {
                DirectoryInfo path = new DirectoryInfo(dirPath);
                string folderName = path.Name;

                Files = path.GetFiles();
                foreach (var file in Files.OrderByDescending(f => f.CreationTimeUtc))
                {
                    using (StreamReader s = file.OpenText())
                    {
                        result.Add(new FileViewModel()
                        {
                            FolderName = folderName,
                            FileFolder = MixCommonHelper.GetFullPath(new string[] { fullPath, folderName }),
                            Filename = file.Name.Substring(0, file.Name.LastIndexOf('.')),
                            Extension = file.Extension,
                            Content = s.ReadToEnd()
                        });
                    }
                }
            }
            return result;
        }

        public List<FileViewModel> GetFiles(string fullPath)
        {
            CreateDirectoryIfNotExist(fullPath);

            FileInfo[] Files;
            List<FileViewModel> result = new List<FileViewModel>();
            foreach (string dirPath in Directory.GetDirectories(fullPath, "*",
                SearchOption.AllDirectories))
            {
                DirectoryInfo path = new DirectoryInfo(dirPath);
                string folderName = path.Name;

                Files = path.GetFiles();
                foreach (var file in Files.OrderByDescending(f => f.CreationTimeUtc))
                {
                    result.Add(new FileViewModel()
                    {
                        FolderName = folderName,
                        FileFolder = MixCommonHelper.GetFullPath(new string[] { fullPath, folderName }),
                        Filename = file.Name.Substring(0, file.Name.LastIndexOf('.')),
                        Extension = file.Extension,
                        //Content = s.ReadToEnd()
                    });
                }
            }
            return result;
        }

        public List<FileViewModel> GetWebFiles(string folder)
        {
            string fullPath = MixCommonHelper.GetFullPath(new string[] {
                    "wwwroot",
                    folder
                });

            CreateDirectoryIfNotExist(fullPath);

            FileInfo[] Files;
            List<FileViewModel> result = new List<FileViewModel>();
            foreach (string dirPath in Directory.GetDirectories(fullPath, "*",
                SearchOption.AllDirectories))
            {
                DirectoryInfo path = new DirectoryInfo(dirPath);
                string folderName = path.ToString().Replace(@"\", "/").Replace("wwwroot", string.Empty);

                Files = path.GetFiles();
                foreach (var file in Files.OrderByDescending(f => f.CreationTimeUtc))
                {
                    result.Add(new FileViewModel()
                    {
                        FolderName = path.Name,
                        FileFolder = folderName,
                        Filename = file.Name.LastIndexOf('.') >= 0 ? file.Name.Substring(0, file.Name.LastIndexOf('.'))
                                    : file.Name,
                        Extension = file.Extension
                    });
                }
            }
            return result;
        }


        public bool SaveWebFile(FileViewModel file)
        {

            try
            {
                string fullPath = $"wwwroot/{file.FileFolder}";
                if (!string.IsNullOrEmpty(file.Filename))
                {
                    CreateDirectoryIfNotExist(fullPath);

                    string fileName = MixCommonHelper.GetFullPath(new string[] { fullPath, file.Filename + file.Extension });
                    if (File.Exists(fileName))
                    {
                        DeleteFile(fileName);
                    }
                    if (!string.IsNullOrEmpty(file.Content))
                    {
                        using (var writer = File.CreateText(fileName))
                        {
                            writer.WriteLine(file.Content); //or .Write(), if you wish
                            return true;
                        }
                    }
                    else
                    {
                        if (IsImage(file.Extension))
                        {
                            // XL
                            ResizeImage(file, "XL");
                            // L
                            ResizeImage(file, "L");
                            // M
                            ResizeImage(file, "M");
                            // S
                            ResizeImage(file, "S");
                            // XS
                            ResizeImage(file, "XS");
                            // XXS
                            ResizeImage(file, "XXS");

                            ResizeImage(file);

                            return true;
                        }
                        else
                        {
                            return SaveFile(file);
                        }
                        
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        
        public FileViewModel SaveWebFile(IFormFile file, string folder)
        {
            try
            {
                string fullPath = $"wwwroot/{folder}";
                string ext = file.FileName[file.FileName.IndexOf('.')..];
                var fileModel = new FileViewModel()
                {
                    Filename = file.FileName.Substring(0, file.FileName.LastIndexOf('.')),
                    Extension = file.FileName.Substring(file.FileName.LastIndexOf('.')),
                    FileFolder = folder,
                    FileStream = GetBase64(file)
                };
                if (IsImage(ext))
                {
                    
                    // XL
                    ResizeImage(fileModel, "XL");
                    // L
                    ResizeImage(fileModel, "L");
                    // M
                    ResizeImage(fileModel, "M");
                    // S
                    ResizeImage(fileModel, "S");
                    // XS
                    ResizeImage(fileModel, "XS");
                    // XXS
                    ResizeImage(fileModel, "XXS");

                    ResizeImage(fileModel);
                }
                else
                {
                    string filename = SaveFile(file, fullPath);
                }
                return fileModel;
            }
            catch
            {
                return null;
            }
        }

        private string GetBase64(IFormFile file)
        {
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                var fileBytes = ms.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }

        private bool IsImage(string extension)
        {
            string[] exts = new string[] { ".img", ".png", ".jpg", ".jpeg" };
            return exts.Contains(extension.ToLower());
        }


        private void ResizeImage(FileViewModel file, string size = null)
        {
            string base64 = file.FileStream.IndexOf(',') >= 0 
                    ? file.FileStream.Split(',')[1]
                    : file.FileStream;
            byte[] bytes = Convert.FromBase64String(base64);
            JObject imageSizes = MixCommonHelper.GetWebConfig<JObject>(WebConfiguration.ImageSizes);
            string fullPath = MixCommonHelper.GetFullPath(new string[] {
                    "wwwroot",
                    file.FileFolder
                });

            using (Image image = Image.Load(bytes))
            {
                if (!string.IsNullOrEmpty(size))
                {
                    int width = imageSizes.GetValue(size).Value<int>("width");
                    int height = (image.Height * width) / image.Width;
                    image.Mutate(x => x.Resize(width, height));
                    image.Save(MixCommonHelper.GetFullPath(new string[] { fullPath, file.Filename + "_" + size + file.Extension }));
                }
                else
                {
                    image.Save(MixCommonHelper.GetFullPath(new string[] { fullPath, file.Filename + file.Extension }));
                }

            }
        }

        public string SaveFile(IFormFile file, string fullPath)
        {
            try
            {
                if (file.Length > 0)
                {
                    CreateDirectoryIfNotExist(fullPath);

                    string filename = file.FileName;
                    string filePath = MixCommonHelper.GetFullPath(new string[] { fullPath, filename });
                    if (File.Exists(filePath))
                    {
                        DeleteFile(filePath);
                    }
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    return filename;
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

        public bool SaveFile(FileViewModel file)
        {
            try
            {
                if (!string.IsNullOrEmpty(file.Filename))
                {
                    CreateDirectoryIfNotExist(file.FileFolder);

                    string fileName = $"{file.Filename}{file.Extension}";
                    if (!string.IsNullOrEmpty(file.FileFolder))
                    {
                        fileName = MixCommonHelper.GetFullPath(new string[] { file.FileFolder, fileName });
                    }
                    if (File.Exists(fileName))
                    {
                        DeleteFile(fileName);
                    }
                    if (!string.IsNullOrEmpty(file.Content))
                    {
                        using (var writer = File.CreateText(fileName))
                        {
                            writer.WriteLine(file.Content); //or .Write(), if you wish
                            writer.Dispose();
                            return true;
                        }
                    }
                    else if(file.FileStream != null)
                    {
                        string base64 = file.FileStream.Split(',')[1];
                        byte[] bytes = Convert.FromBase64String(base64);
                        using (var writer = File.Create(fileName))
                        {
                            writer.Write(bytes, 0, bytes.Length);
                            return true;
                        }
                    }
                    else
                    {
                        File.CreateText(fileName);
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public void UnZipFile(string filePath, string webFolder)
        {
            try
            {
                ZipFile.ExtractToDirectory(filePath, webFolder);
            }
            catch
            {
                //throw;
            }
        }

        public bool UnZipFile(FileViewModel file)
        {
            string filePath = MixCommonHelper.GetFullPath(new string[]
            {
                 "wwwroot",
                file.FileFolder,
                $"{file.Filename}{file.Extension}"
            });
            string webFolder = MixCommonHelper.GetFullPath(new string[]
            {
                "wwwroot",
                file.FileFolder
            });
            try
            {
                ZipFile.ExtractToDirectory(filePath, webFolder);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string ZipFolder(string tmpPath, string outputPath, string fileName)
        {
            try
            {
                //string tmpPath = $"wwwroot/Exports/temp/{fileName}-{DateTime.UtcNow.ToShortDateString()}";
                string outputFile = $"wwwroot/{outputPath}/{fileName}.zip";
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
    }
}