using System;

namespace Mix.Heart.Entity
{
    public class Entity : Entity<Guid>, IEntity
    {
    }

    public class Entity<TPrimaryKey> : IEntity<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
    }
}
