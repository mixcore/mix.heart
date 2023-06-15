using System;
using System.IO;

namespace Mix.Heart.Models
{
    public class FileModel
    {
        #region Properties
        public string FolderName { get; set; }

        public string FileFolder { get; set; }

        public string Filename { get; set; }

        public string Extension { get; set; }

        public string Content { get; set; }

        public string FileBase64 { get; set; }
        public Stream FileStream{ get; set; }

        public string FullPath
        {
            get
            {
                return $"{FileFolder}/{Filename}{Extension}"; ;
            }
        }

        public string WebPath
        {
            get
            {

                return FullPath.Replace("wwwroot", string.Empty);
            }
        }

        #endregion

        public FileModel()
        {
        }

        public FileModel(string fileName, Stream fileStream, string folder)
        {
            Filename = fileName[0..fileName.LastIndexOf('.')];//Math.Min(fileName.LastIndexOf('.'), 40)];
            Extension = fileName[fileName.LastIndexOf('.')..];
            FileFolder = folder;
            FileStream = fileStream;
        }
    }
}
