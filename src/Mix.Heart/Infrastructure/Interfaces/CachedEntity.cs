using System;

namespace Mix.Heart.Infrastructure.Interfaces
{
    public abstract class CachedEntity
    {
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
