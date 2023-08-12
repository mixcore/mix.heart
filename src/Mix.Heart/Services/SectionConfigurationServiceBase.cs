using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Mix.Heart.Constants;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Extensions;
using Mix.Heart.Helpers;
using Mix.Heart.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;

namespace Mix.Heart.Services
{
    public class SectionConfigurationServiceBase<T>
        where T : class
    {
        private string filePath;
        public string AesKey { get; set; }
        public T AppSettings { get; set; }
        protected string FilePath { get => filePath; set => filePath = value; }
        protected DateTime LastModified { get; private set; }
        protected DateTime LastSaved { get; private set; }
        protected readonly FileSystemWatcher watcher = new();
        protected readonly IConfiguration _configuration;
        private readonly string _section;

        public SectionConfigurationServiceBase(IConfiguration configuration, string section, string filePath)
        {
            _configuration = configuration;
            _section = section;
            FilePath = filePath;
            LoadAppSettings();
            WatchFile();
        }

        public TValue GetConfig<TValue>(string name, TValue defaultValue = default)
        {
            try
            {
                return (TValue)ReflectionHelper.GetPropertyValue<T>(AppSettings, name);
            }
            catch(Exception ex)
            {
                throw new MixException(MixErrorStatus.Badrequest, ex);
            }
        }

        public TValue GetEnumConfig<TValue>(string name)
        {
            var strResult = GetConfig<string>(name);
            Enum.TryParse(typeof(TValue), strResult, true, out object result);
            return result != null ? (TValue)result : default;
        }

        public void SetConfig<TValue>(string name, TValue value)
        {
            ReflectionHelper.SetPropertyValue<T>(AppSettings, new EntityPropertyModel()
            {
                PropertyName = name,
                PropertyValue = value
            });
            SaveSettings();
        }

        public virtual bool SaveSettings()
        {
            var settings = MixFileHelper.GetFileByFullName($"{FilePath}{MixFileExtensions.Json}", true, "{}");
            if (settings != null)
            {
                settings.Content = (new JObject(new JProperty(_section, AppSettings))).ToString(Formatting.None);
                var result = MixFileHelper.SaveFile(settings);

                LastSaved = DateTime.UtcNow;
                return result;
            }
            else
            {
                return false;
            }
        }

        protected void WatchFile()
        {
            watcher.Path = FilePath[..FilePath.LastIndexOf('/')];
            watcher.Filter = "";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (LastSaved > LastModified)
            {
                Thread.Sleep(500);
                LoadAppSettings();
            }
        }

        protected virtual void LoadAppSettings()
        {
            AppSettings = CreateInstance();
            _configuration.GetSection(_section).Bind(AppSettings);
            LastModified = DateTime.UtcNow;
            LastSaved = DateTime.UtcNow;
        }

        protected T CreateInstance()
        {
            var settingType = typeof(T);
            var contextCtorInfo = settingType.GetConstructor(new Type[] { });

            if (contextCtorInfo == null)
            {
                throw new MixException(
                        MixErrorStatus.ServerError,
                        $"{settingType}: Contructor Parameterless Notfound");
            }
            return (T)Activator.CreateInstance(settingType);
        }
    }
}
