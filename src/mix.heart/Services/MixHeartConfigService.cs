using Mix.Heart.Constants;
using Mix.Heart.Enums;
using Mix.Heart.Models;
using Mix.Heart.Services;

namespace Mix.Shared.Services
{
    public class MixHeartConfigService : ConfigurationServiceBase<MixHeartConfigurationModel>
    {

        #region Instance
        /// <summary>
        /// The synchronize root
        /// </summary>
        protected static readonly object syncRoot = new object();

        /// <summary>
        /// The instance
        /// </summary>
        private static MixHeartConfigService instance;

        public static MixHeartConfigService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        public MixHeartConfigService()
            : base(MixHeartConstants.ConfigFilePath)
        {
        }

        public MixDatabaseProvider DatabaseProvider
        {
            get => AppSettings.DatabaseProvider;
        }

        public string GetConnectionString(string name)
        {
            switch (name)
            {
                case MixHeartConstants.CACHE_CONNECTION:
                    return AppSettings.CacheConnection;
                default:
                    return string.Empty;
            }
        }

        public void SetConnectionString(string name, string value)
        {
            switch (name)
            {
                case MixHeartConstants.CACHE_CONNECTION:
                    AppSettings.CacheConnection = value;
                    break;
                default:
                    break;
            }
            SaveSettings();
        }
    }
}
