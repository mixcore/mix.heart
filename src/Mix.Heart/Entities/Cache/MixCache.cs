using Mix.Heart.Enums;
using System;

namespace Mix.Heart.Entities.Cache {
  public class MixCache : Entity<string> {
    public string Value { get; set; }
    public DateTime? ExpiredDateTime { get; set; }
    public string ModifiedBy { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public int Priority { get; set; }
    public MixCacheStatus Status { get; set; }
  }
}
