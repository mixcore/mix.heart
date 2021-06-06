using System;

namespace Mix.Heart.Entity {
  public interface IEntity : IEntity<Guid> {}

  public interface IEntity<TPrimaryKey>
      : IHasPrimaryKey<TPrimaryKey> where TPrimaryKey : IComparable {}
}
