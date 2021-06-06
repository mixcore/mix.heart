using System;
using System.Threading.Tasks;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
    {
        private bool _isRoot;

        protected virtual void BeginUow(ref UnitOfWorkInfo unitOfWorkInfo)
        {
            if (unitOfWorkInfo != null)
            {
                _isRoot = false;
                return;
            };

            Console.WriteLine("Unit of work starting");

            _isRoot = true;

            var dbContextType = typeof(TDbContext);
            var contextCtorInfo = dbContextType.GetConstructor(new Type[] { });

            if (contextCtorInfo == null)
            {
                throw new NullReferenceException();
            }

            var dbContext = (TDbContext)contextCtorInfo.Invoke(new object[] { });

            var dbContextTransaction = dbContext.Database.BeginTransaction();

            unitOfWorkInfo = new UnitOfWorkInfo();
            unitOfWorkInfo.SetDbContext(dbContext);
            unitOfWorkInfo.SetTransaction(dbContextTransaction);

            Console.WriteLine("Unit of work started");
        }

        protected virtual void CompleteUow(UnitOfWorkInfo unitOfWorkInfo)
        {
            if (!_isRoot)
            {
                return;
            };

            unitOfWorkInfo.Complete();

            _isRoot = false;

            Console.WriteLine("Unit of work completed.");
        }

        protected virtual void CloseUow(UnitOfWorkInfo unitOfWorkInfo)
        {
            unitOfWorkInfo.Close();
        }

        protected virtual async Task CompleteUowAsync(UnitOfWorkInfo unitOfWorkInfo)
        {
            if (!_isRoot)
            {
                return;
            };

            await unitOfWorkInfo.CompleteAsync();
            unitOfWorkInfo.Close();

            _isRoot = false;
        }
    }
}
