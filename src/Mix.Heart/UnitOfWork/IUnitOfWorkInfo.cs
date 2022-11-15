using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.UnitOfWork
{
    public interface IUnitOfWorkInfo : IAsyncDisposable
    {
        DbContext ActiveDbContext { get; }
        IDbContextTransaction ActiveTransaction { get; }
        void Begin();
        void Close();
        Task CloseAsync();
        void Complete();
        Task CompleteAsync(CancellationToken cancellationToken = default);
        void Rollback();
        Task RollbackAsync(CancellationToken cancellationToken = default);
        void SetDbContext(DbContext dbContext);
        void SetTransaction(IDbContextTransaction dbContextTransaction);
    }
}