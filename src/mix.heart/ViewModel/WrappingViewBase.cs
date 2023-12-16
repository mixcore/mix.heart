using Microsoft.EntityFrameworkCore;
using Mix.Heart.UnitOfWork;
using System;

namespace Mix.Heart.ViewModel
{
    public abstract class WrappingViewBase<TDbContext> where TDbContext : DbContext
    {
        public virtual void Execute()
        {
            UnitOfWorkInfo uowInfo = null;
            try
            {
                BeginUow(ref uowInfo);
                SaveGroupView(uowInfo);
            }
            catch (Exception)
            {
                uowInfo.Close();
                throw;
            }
            finally
            {
                uowInfo.Complete();
            }
        }

        protected virtual void BeginUow(ref UnitOfWorkInfo uowInfo)
        {
            var dbContextType = typeof(TDbContext);
            var contextCtorInfo = dbContextType.GetConstructor(new Type[] { });

            if (contextCtorInfo == null)
            {
                throw new NullReferenceException();
            }

            var dbContext = (TDbContext)contextCtorInfo.Invoke(new object[] { });
            uowInfo = new UnitOfWorkInfo(dbContext);
        }

        protected abstract void SaveGroupView(UnitOfWorkInfo uowInfo);
    }
}
