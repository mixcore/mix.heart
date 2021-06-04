using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entity;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TPrimaryKey, TEntity, TDbContext> : IViewModel<TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {
        public virtual TPrimaryKey Id { get; set; }
    }
}
