using System;
using System.Threading.Tasks;
using Mix.Heart.Infrastructure.Interfaces;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
    {
        private bool _isRoot;

        protected ViewModelBase()
        {
        }

        protected virtual void BeginUow(UnitOfWorkInfo uowInfo = null, IMixMediator consumer = null)
        {
            _consumer ??= consumer;
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
                _repository ??= new CommandRepository<TDbContext, TEntity, TPrimaryKey>(_unitOfWorkInfo);
                _repository.SetUowInfo(_unitOfWorkInfo);
                return;
            };

            InitRootUow();

        }

        private void InitRootUow()
        {
            _isRoot = true;

            var dbContext = _context ?? InitDbContext();
            var dbContextTransaction = dbContext.Database.BeginTransaction();
            _unitOfWorkInfo = new UnitOfWorkInfo();
            _unitOfWorkInfo.SetDbContext(dbContext);
            _unitOfWorkInfo.SetTransaction(dbContextTransaction);
            _repository ??= new CommandRepository<TDbContext, TEntity, TPrimaryKey>(_unitOfWorkInfo);
            _repository.SetUowInfo(_unitOfWorkInfo);
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
