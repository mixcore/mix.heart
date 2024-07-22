using Mix.Heart.Constants;
using Mix.Heart.Extensions;
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
        public JObject RawSettings;
        protected bool _isEncrypt;
        public string AesKey { get; set; }
        public string SectionName { get; set; }
        public T AppSettings { get; set; }
        protected string FilePath { get => filePath; set => filePath = value; }

        public ConfigurationServiceBase(string filePath, string sectionName = null)
        {
            FilePath = filePath;
            SectionName = sectionName;
            LoadAppSettings();
        }
        public ConfigurationServiceBase(string filePath, bool isEncrypt)
        {
            FilePath = filePath;
            _isEncrypt = isEncrypt;
            LoadAppSettings();
        }

        public TValue GetConfig<TValue>(string name, TValue defaultValue = default)
        {
            var result = RawSettings[name];
            return result != null ? result.Value<TValue>() : defaultValue;
        }

        public TValue GetConfig<TValue>(string culture, string name, TValue defaultValue = default)
        {
            JToken result = null;
            if (!string.IsNullOrEmpty(culture) && RawSettings[culture] != null)
            {
                result = RawSettings[culture][name];
            }
            return result != null ? result.Value<TValue>() : defaultValue;
        }

        public TValue GetEnumConfig<TValue>(string name)
        {
            Enum.TryParse(typeof(TValue), RawSettings[name]?.Value<string>(), true, out object result);
            return result != null ? (TValue)result : default;
        }

        public void SetConfig<TValue>(string name, TValue value)
        {
            RawSettings[name] = value != null ? JToken.FromObject(value) : null;
            AppSettings = RawSettings.ToObject<T>();
            SaveSettings();
        }

        public void SetConfig<TValue>(string culture, string name, TValue value)
        {
            RawSettings[culture][name] = value.ToString();
            AppSettings = RawSettings.ToObject<T>();
            SaveSettings();
        }

        public virtual bool SaveSettings()
        {
            var settings = MixFileHelper.GetFileByFullName($"{FilePath}{MixFileExtensions.Json}", true, "{}");
            if (settings != null)
            {
                settings.Content = string.IsNullOrEmpty(SectionName)
                                    ? ReflectionHelper.ParseObject(AppSettings).ToString(Formatting.None)
                                    : ReflectionHelper.ParseObject(
                                        new JObject(
                                            new JProperty(SectionName, ReflectionHelper.ParseObject(AppSettings))))
                                    .ToString(Formatting.None);
                if (_isEncrypt)
                {
                    settings.Content = AesEncryptionHelper.EncryptString(settings.Content, AesKey);
                }
                var result = MixFileHelper.SaveFile(settings);
                return result;
            }
            else
            {
                return false;
            }
        }

        protected virtual void LoadAppSettings()
        {
            var settings = MixFileHelper.GetFileByFullName($"{FilePath}{MixFileExtensions.Json}", true, "{}");
            string content = string.IsNullOrWhiteSpace(settings.Content) ? "{}" : settings.Content;

            if (_isEncrypt)
            {
                if (!content.IsJsonString())
                {
                    content = AesEncryptionHelper.DecryptString(content, AesKey);
                }
                else
                {
                    // Encrypt and save to setting file if not encrypted
                    settings.Content = AesEncryptionHelper.EncryptString(content, AesKey);
                    MixFileHelper.SaveFile(settings);
                }

            }
            RawSettings = JObject.Parse(content);
            AppSettings = string.IsNullOrEmpty(SectionName) ? RawSettings.ToObject<T>()
                : RawSettings[SectionName].ToObject<T>();
        }
    }
}
