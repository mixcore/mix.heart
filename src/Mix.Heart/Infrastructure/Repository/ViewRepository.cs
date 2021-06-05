using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entity;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public class ViewRepository<TDbContext, TEntity> : RepositoryBase<TDbContext>
        where TDbContext : DbContext
        where TEntity: class
    {
        public ViewRepository(TDbContext dbContext) : base(dbContext)
        {
        }

        public virtual object GetById(object id)
        {
            return Context.Set<TEntity>().Find(id);
        }

        public Task<TEntity> GetSingleModelAsync(Expression<Func<TEntity, bool>> predicate) 
        {
            throw new NotImplementedException();
        }
    }
}
