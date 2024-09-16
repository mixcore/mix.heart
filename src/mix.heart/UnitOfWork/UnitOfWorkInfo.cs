using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.UnitOfWork
{
public class UnitOfWorkInfo(DbContext dbContext) : IUnitOfWorkInfo
{
    public DbContext ActiveDbContext {
        get;
        private set;
    } = dbContext;

    public IDbContextTransaction ActiveTransaction {
        get;
        private set;
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

    public void Close()
    {
        ActiveTransaction?.Dispose();
    }

    public async Task CloseAsync()
    {
        if (ActiveTransaction != null)
        {
            await ActiveTransaction.DisposeAsync();
        }
    }

    public void Complete()
    {
        if (ActiveDbContext != null && ActiveTransaction != null)
        {
            ActiveDbContext.SaveChanges();
            ActiveTransaction.Commit();

            ActiveTransaction.Dispose();
            ActiveDbContext.Dispose();

            ActiveTransaction = null;
            ActiveDbContext = null;
        }
    }

    public void Rollback()
    {
        if (ActiveDbContext != null && ActiveTransaction != null)
        {
            ActiveTransaction.Rollback();

            ActiveTransaction.Dispose();
            ActiveDbContext.Dispose();

            ActiveTransaction = null;
            ActiveDbContext = null;
        }
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (ActiveDbContext != null && ActiveTransaction != null)
        {
            await ActiveDbContext.SaveChangesAsync(cancellationToken);
            await ActiveTransaction.CommitAsync(cancellationToken);

            await ActiveTransaction.DisposeAsync();
            await ActiveDbContext.DisposeAsync();

            ActiveTransaction = null;
            ActiveDbContext = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (ActiveDbContext != null && ActiveTransaction != null)
        {
            await ActiveTransaction.RollbackAsync(cancellationToken);

            await ActiveTransaction.DisposeAsync();
            await ActiveDbContext.DisposeAsync();

            ActiveTransaction = null;
            ActiveDbContext = null;
        }
    }

    public void Dispose()
    {
        ActiveTransaction?.Dispose();
        ActiveDbContext?.Dispose();

        GC.SuppressFinalize(this);
        GC.WaitForPendingFinalizers();
    }
}
}
