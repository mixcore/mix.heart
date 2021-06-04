using Microsoft.EntityFrameworkCore;

namespace Mix.Heart.Repository
{
    public class WriteRepository<TDbContext> : RepositoryBase<TDbContext>
        where TDbContext : DbContext
    {
        public WriteRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
