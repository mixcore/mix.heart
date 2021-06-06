using Microsoft.EntityFrameworkCore;

namespace Mix.Heart.Repository
{
    public class CommandRepository<TDbContext, TEntity> : QueryRepository<TDbContext, TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
        public CommandRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
