using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.UnitOfWork
{
    public interface IUnitOfWorkInfo: IAsyncDisposable
    {
        DbContext ActiveDbContext { get; }
        IDbContextTransaction ActiveTransaction { get; }

        void Begin();
        void Close();
        Task CloseAsync();
        void Complete();
        Task CompleteAsync();
        void Rollback();
        Task RollbackAsync();
        void SetDbContext(DbContext dbContext);
        void SetTransaction(IDbContextTransaction dbContextTransaction);
    }
}