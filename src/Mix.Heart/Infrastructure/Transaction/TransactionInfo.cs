using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mix.Heart.Transaction
{
    public class TransactionInfo
    {
        public TransactionInfo()
        {
        }

        public TransactionInfo(DbContext dbContext)
        {
            ActiveDbContext = dbContext;
        }

        public DbContext ActiveDbContext { get; private set; }

        public IDbContextTransaction ActiveTransaction { get; private set; }

        public void SetActiveTransaction(IDbContextTransaction dbContextTransaction)
        {
            ActiveTransaction = dbContextTransaction;
        }
    }
}
