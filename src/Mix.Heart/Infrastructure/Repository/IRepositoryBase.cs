using Microsoft.EntityFrameworkCore;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.Repository
{
    public interface IRepositoryBase<TDbContext> where TDbContext : DbContext
    {
        TDbContext _dbContext { get; }
        UnitOfWorkInfo _unitOfWorkInfo { get; set; }
        TDbContext Context { get; }

        void SetUow(UnitOfWorkInfo unitOfWorkInfo);
    }
}