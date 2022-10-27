using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
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
                await ActiveTransaction.DisposeAsync();
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
        public async Task CompleteAsync()
        {
            if (ActiveDbContext != null && ActiveTransaction != null)
            {
                await ActiveDbContext.SaveChangesAsync();
                await ActiveTransaction.CommitAsync();
                await ActiveTransaction.DisposeAsync();
                ActiveTransaction = null;
            }
        }
        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public async Task RollbackAsync()
        {
            if (ActiveDbContext != null && ActiveTransaction != null)
            {
                await ActiveTransaction.RollbackAsync();
                await ActiveTransaction.DisposeAsync();
                ActiveTransaction = null;
            }
        }

        public ValueTask DisposeAsync()
        {
            Task.Run(() =>
            {
                ActiveDbContext?.DisposeAsync();
            }).ContinueWith((result) =>
            {
                GC.SuppressFinalize(this);
                GC.WaitForPendingFinalizers();
            });
            return ValueTask.CompletedTask;
        }
    }
}
