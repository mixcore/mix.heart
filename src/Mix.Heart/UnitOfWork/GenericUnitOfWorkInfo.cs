using Microsoft.EntityFrameworkCore;

namespace Mix.Heart.UnitOfWork
{
    public class GenericUnitOfWorkInfo<T>: UnitOfWorkInfo
        where T : DbContext
    {
        public T DbContext => (T)ActiveDbContext;
        public GenericUnitOfWorkInfo(T dbContext): base(dbContext)
        {
        }
    }
}
