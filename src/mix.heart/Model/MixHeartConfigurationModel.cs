﻿using Mix.Heart.Enums;

namespace Mix.Heart.Models
{
    public class MixHeartConfigurationModel
    {
        public string CacheConnection { get; set; }
        public bool IsCache { get; set; }
        public MixCacheMode CacheMode { get; set; }
        public MixDatabaseProvider DatabaseProvider { get; set; }
        public string CacheFolder { get; set; }
        public MixHeartConfigurationModel()
        {
        }
    }
}