using Microsoft.EntityFrameworkCore;

namespace Mix.Heart.UnitOfWork
{
public class UnitOfWorkInfo<T>(T dbContext) : UnitOfWorkInfo(dbContext) where T : DbContext
{
    public T DbContext => (T)ActiveDbContext;
}
}
