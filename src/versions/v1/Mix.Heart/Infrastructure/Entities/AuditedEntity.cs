using System;

namespace Mix.Heart.Infrastructure.Entities
{
    public abstract class AuditedEntity
    {
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
