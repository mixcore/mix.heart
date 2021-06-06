using Microsoft.EntityFrameworkCore;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.Repository
{
public interface IRepositoryBase<TDbContext> where TDbContext : DbContext
{
    UnitOfWorkInfo _unitOfWorkInfo {
        get;
        set;
    }
    TDbContext Context {
        get;
    }

    void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo);
}
}