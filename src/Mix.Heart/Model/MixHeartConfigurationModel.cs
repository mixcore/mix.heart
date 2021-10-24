using Mix.Heart.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mix.Heart.Model
{
    public class MixHeartConfigurationModel
    {
        public string ConnectionString { get; set; }
        public bool IsCache { get; set; }
        public MixCacheMode Mode { get; set; }
        public MixCacheDbProvider DbProvider { get; set; }
        public string CacheFolder { get; set; }

        public MixHeartConfigurationModel()
        {

        }
    }
}
