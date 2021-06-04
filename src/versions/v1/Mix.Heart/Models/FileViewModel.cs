using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace Mix.Heart.Models
{
    public class FileViewModel
    {
        [JsonProperty("fullPath")]
        public string FullPath
        {
            get
            {
                return $"{FileFolder}/{Filename}{Extension}"; ;
            }
        }

        [JsonProperty("webPath")]
        public string WebPath
        {
            get
            {

                return FullPath.Replace("wwwroot", string.Empty);
            }
        }

        [JsonProperty("folderName")]
        public string FolderName { get; set; }

        [JsonProperty("fileFolder")]
        public string FileFolder { get; set; }

        [JsonProperty("fileName")]
        public string Filename { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("fileStream")]
        public string FileStream { get; set; }

        public FileViewModel()
        {
        }

        public FileViewModel(IFormFile file, string folder)
        {
            Filename = file.FileName.Substring(0, file.FileName.LastIndexOf('.'));
            Extension = file.FileName.Substring(file.FileName.LastIndexOf('.'));
            FileFolder = folder;
        }
    }
}
