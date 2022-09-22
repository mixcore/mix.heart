﻿using Mix.Heart.Constants;
using Mix.Heart.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;

namespace Mix.Heart.Services
{
    public class ConfigurationServiceBase<T>
    {
        private string filePath;
        private JObject _obj;
        protected bool _isEncrypt;
        public string AesKey { get; set; }
        public T AppSettings { get; set; }
        protected string FilePath { get => filePath; set => filePath = value; }

        protected readonly FileSystemWatcher watcher = new();

        public ConfigurationServiceBase(string filePath)
        {
            FilePath = filePath;
            LoadAppSettings();
            WatchFile();
        }
        public ConfigurationServiceBase(string filePath, bool isEncrypt)
        {
            FilePath = filePath;
            _isEncrypt = isEncrypt;
            LoadAppSettings();
            WatchFile();
        }

        public TValue GetConfig<TValue>(string name, TValue defaultValue = default)
        {
            var result = _obj[name];
            return result != null ? result.Value<TValue>() : defaultValue;
        }

        public TValue GetConfig<TValue>(string culture, string name, TValue defaultValue = default)
        {
            JToken result = null;
            if (!string.IsNullOrEmpty(culture) && _obj[culture] != null)
            {
                result = _obj[culture][name];
            }
            return result != null ? result.Value<TValue>() : defaultValue;
        }

        public TValue GetEnumConfig<TValue>(string name)
        {
            Enum.TryParse(typeof(TValue), _obj[name]?.Value<string>(), true, out object result);
            return result != null ? (TValue)result : default;
        }

        public void SetConfig<TValue>(string name, TValue value)
        {
            _obj[name] = value != null ? JToken.FromObject(value) : null;
            AppSettings = _obj.ToObject<T>();
            SaveSettings();
        }

        public void SetConfig<TValue>(string culture, string name, TValue value)
        {
            _obj[culture][name] = value.ToString();
            AppSettings = _obj.ToObject<T>();
            SaveSettings();
        }

        public bool SaveSettings()
        {
            var settings = MixFileHelper.GetFileByFullName($"{FilePath}{MixFileExtensions.Json}", true, "{}");
            if (settings != null)
            {
                settings.Content = ReflectionHelper.ParseObject(AppSettings).ToString(Formatting.None);
                if (_isEncrypt)
                {
                    settings.Content = AesEncryptionHelper.EncryptString(settings.Content, AesKey);
                }
                return !string.IsNullOrEmpty(MixFileHelper.SaveFile(settings));
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
            Thread.Sleep(500);
            LoadAppSettings();
        }

        protected virtual void LoadAppSettings()
        {
            var settings = MixFileHelper.GetFileByFullName($"{FilePath}{MixFileExtensions.Json}", true, "{}");
            string content = string.IsNullOrWhiteSpace(settings.Content) ? "{}" : settings.Content;

            if (_isEncrypt && !content.StartsWith('{'))
            {
                content = AesEncryptionHelper.DecryptString(content, AesKey);
            }

            _obj = JObject.Parse(content);
            AppSettings = _obj.ToObject<T>();
        }
    }
}
