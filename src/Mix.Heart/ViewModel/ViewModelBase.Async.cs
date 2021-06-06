using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Mix.Heart.Entity;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.ViewModel
{
public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
    where TPrimaryKey : IComparable
    where TEntity : class, IEntity<TPrimaryKey>
    where TDbContext : DbContext
{

}
}
