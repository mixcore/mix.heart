using Microsoft.EntityFrameworkCore;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.Repository
{
public interface IRepositoryBase<TDbContext> where TDbContext : DbContext
{
    UnitOfWorkInfo UowInfo {
        get;
        set;
    }

    TDbContext Context {
        get;
    }

    void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo);
}
}