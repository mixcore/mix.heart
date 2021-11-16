using Microsoft.AspNetCore.Http;

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

        public string FileStream { get; set; }

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
        public FileModel(IFormFile file, string folder)
        {
            Filename = file.FileName[0..file.FileName.LastIndexOf('.')];
            Extension = file.FileName[file.FileName.LastIndexOf('.')..];
            FileFolder = folder;
        }
    }
}
