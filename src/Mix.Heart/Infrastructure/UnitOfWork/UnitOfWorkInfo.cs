using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public void Close()
        {
            ActiveDbContext.Dispose();
            ActiveTransaction?.Dispose();
        }
        
        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public async Task CloseAsync()
        {
            await ActiveDbContext.DisposeAsync();
            await ActiveTransaction.DisposeAsync();
        }

        /// <summary>
        /// TODO: implement multiple db context
        /// </summary>
        public void Complete()
        {
            ActiveDbContext.SaveChanges();
            ActiveTransaction.Commit();

            Close();
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
