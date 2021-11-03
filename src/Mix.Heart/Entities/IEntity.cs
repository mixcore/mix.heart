using System;

namespace Mix.Heart.Entities
{
    public interface IEntity : IEntity<Guid>
    {
    }

    public interface IEntity<TPrimaryKey> : IHasPrimaryKey<TPrimaryKey>
        where TPrimaryKey: IComparable
    {
    }
}
