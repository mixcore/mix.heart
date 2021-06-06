using System;
using System.Threading.Tasks;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
    {
        private bool _isRoot;

        protected virtual void BeginUow(UnitOfWorkInfo uowInfo = null)
        {
            _unitOfWorkInfo ??= uowInfo;
            if (_unitOfWorkInfo != null)
            {
                _isRoot = false;
                if (_unitOfWorkInfo.ActiveTransaction == null)
                {

                    _unitOfWorkInfo.SetTransaction(
                        _unitOfWorkInfo.ActiveDbContext.Database.CurrentTransaction
                        ?? _unitOfWorkInfo.ActiveDbContext.Database.BeginTransaction());
                }
                return;
            };

            Console.WriteLine("Unit of work starting");

            InitRootUow();

        }

        private void InitRootUow()
        {
            _isRoot = true;

            var dbContext = InitDbContext();

            var dbContextTransaction = dbContext.Database.BeginTransaction();

            _unitOfWorkInfo = new UnitOfWorkInfo();
            _unitOfWorkInfo.SetDbContext(dbContext);
            _unitOfWorkInfo.SetTransaction(dbContextTransaction);

            Console.WriteLine("Unit of work started");
        }

        protected virtual void CompleteUow()
        {
            if (!_isRoot)
            {
                return;
            };

            _unitOfWorkInfo.Complete();

            _isRoot = false;

            Console.WriteLine("Unit of work completed.");
        }

        protected virtual void CloseUow()
        {
            _unitOfWorkInfo.Close();
        }

        protected virtual async Task CompleteUowAsync()
        {
            if (!_isRoot)
            {
                return;
            };

            await _unitOfWorkInfo.CompleteAsync();
            _unitOfWorkInfo.Close();

            _isRoot = false;
        }

        private TDbContext InitDbContext()
        {
            var dbContextType = typeof(TDbContext);
            var contextCtorInfo = dbContextType.GetConstructor(new Type[] { });

            if (contextCtorInfo == null)
            {
                throw new NullReferenceException();
            }

            return (TDbContext)contextCtorInfo.Invoke(new object[] { });
        }
    }
}
