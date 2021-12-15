using System;

namespace Mix.Heart.Entities
{
    public abstract class Entity : Entity<Guid>, IEntity
    {
    }

    public class Entity<TPrimaryKey> : IEntity<TPrimaryKey> where TPrimaryKey : IComparable
    {
        public TPrimaryKey Id { get; set; }
    }
}
