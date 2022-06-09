using Microsoft.EntityFrameworkCore;

namespace Mix.Heart.UnitOfWork
{
    public class UnitOfWorkInfo<T>: UnitOfWorkInfo
        where T : DbContext
    {
        public T DbContext => (T)ActiveDbContext;
        public UnitOfWorkInfo(T dbContext): base(dbContext)
        {
        }
    }
}
