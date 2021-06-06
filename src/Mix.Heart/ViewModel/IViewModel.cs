using System;
using Mix.Heart.Entity;

namespace Mix.Heart.ViewModel
{
public interface IViewModel : IViewModel<Guid>
{
}

public interface IViewModel<TPrimaryKey> : IHasPrimaryKey<TPrimaryKey>
    where TPrimaryKey : IComparable
{
}
}
