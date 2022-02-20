using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace Mix.Heart.UnitOfWork
{
    public class UnitOfWorkInfo
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
            ActiveDbContext.SaveChanges();
            ActiveTransaction.Commit();
        }

        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public async Task CompleteAsync()
        {
            await ActiveDbContext.SaveChangesAsync();
            await ActiveTransaction.CommitAsync();
        }
    }
}
