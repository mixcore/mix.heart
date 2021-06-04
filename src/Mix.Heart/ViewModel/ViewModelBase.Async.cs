using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Mix.Heart.Entity;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TPrimaryKey, TEntity, TDbContext>
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {
        public virtual async Task<int> SaveAsync(UnitOfWorkInfo transactionInfo = null)
        {
            try
            {
                BeginUow(ref transactionInfo);
            }
            catch (Exception)
            {
                CloseUow(transactionInfo);
                throw;
            }
            finally
            {
                await CompleteUowAsync(transactionInfo);
            }

            return 1;
        }
    }
}
