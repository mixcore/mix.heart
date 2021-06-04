using Microsoft.EntityFrameworkCore;

namespace Mix.Heart.Repository
{
    public class ViewRepository<TDbContext> : RepositoryBase<TDbContext>
        where TDbContext : DbContext
    {
        public ViewRepository(TDbContext dbContext) : base(dbContext)
        {
        }

        public virtual object GetById(object id)
        {
            return Context.Set<object>().Find(id);
        }
    }
}
