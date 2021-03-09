using Mix.Heart.Domain.Enums;
using System;

namespace Mix.Heart.Domain.Entities
{
    public partial class MixCache
    {
        public string Id { get; set; }
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