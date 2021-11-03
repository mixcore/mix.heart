using Mix.Heart.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mix.Heart.Model {
  public class MixHeartConfigurationModel {
    public string ConnectionString { get; set; }
    public bool IsCache { get; set; }
    public MixCacheMode CacheMode { get; set; }
    public MixCacheDbProvider DatabaseProvider { get; set; }
    public string CacheFolder { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
    public MixHeartConfigurationModel() {}
  }

  public class ConnectionStrings {
    public string CacheConnection { get; set; }
    public string AuditLogConnection { get; set; }
  }
}
