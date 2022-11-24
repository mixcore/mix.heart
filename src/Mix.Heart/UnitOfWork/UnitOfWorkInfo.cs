using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.UnitOfWork
{
    public class UnitOfWorkInfo : IUnitOfWorkInfo
    {
        public DbContext ActiveDbContext { get; private set; }

        public IDbContextTransaction ActiveTransaction { get; private set; }

        public UnitOfWorkInfo(DbContext dbContext)
        {
            ActiveDbContext = dbContext;
        }

        public void SetDbContext(DbContext dbContext)
        {
            ActiveDbContext = dbContext;
        }

        public void SetTransaction(IDbContextTransaction dbContextTransaction)
        {
            ActiveTransaction = dbContextTransaction;
        }

        public void Begin()
        {
            if (ActiveTransaction == null)
            {
                SetTransaction(
                    ActiveDbContext.Database.CurrentTransaction
                    ?? ActiveDbContext.Database.BeginTransaction());
            }
        }

        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public void Close()
        {
            //ActiveDbContext.Dispose();
            ActiveTransaction?.Dispose();
        }

        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public async Task CloseAsync()
        {
            //if (ActiveDbContext != null)
            //    await ActiveDbContext.DisposeAsync();
            if (ActiveTransaction != null)
            {
                await ActiveTransaction.DisposeAsync();
            }
        }

        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public void Complete()
        {
            if (ActiveDbContext != null && ActiveTransaction != null)
            {
                ActiveDbContext.SaveChanges();
                ActiveTransaction?.Commit();
                ActiveTransaction?.Dispose();
                ActiveTransaction = null;
            }
        }
        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public void Rollback()
        {
            if (ActiveDbContext != null && ActiveTransaction != null)
            {
                ActiveTransaction?.Rollback();
                ActiveTransaction?.Dispose();
                ActiveTransaction = null;
            }
        }

        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            if (ActiveDbContext != null && ActiveTransaction != null)
            {
                await ActiveDbContext.SaveChangesAsync(cancellationToken);
                await ActiveTransaction.CommitAsync(cancellationToken);
                await ActiveTransaction.DisposeAsync();
                ActiveTransaction = null;
            }
        }
        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (ActiveDbContext != null && ActiveTransaction != null)
            {
                await ActiveTransaction.RollbackAsync(cancellationToken);
                await ActiveTransaction.DisposeAsync();
                ActiveTransaction = null;
            }
        }

        public void Dispose()
        {
            ActiveDbContext?.Dispose();
            GC.SuppressFinalize(this);
            GC.WaitForPendingFinalizers();
        }
    }
}
