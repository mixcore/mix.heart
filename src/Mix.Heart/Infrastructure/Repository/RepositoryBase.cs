using Microsoft.EntityFrameworkCore;

namespace Mix.Heart.Repository
{
    public abstract class RepositoryBase<TDbContext> where TDbContext : DbContext
    {
        private TDbContext _dbContext;

        protected RepositoryBase(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual TDbContext Context => _dbContext;
    }
}
