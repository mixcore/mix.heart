using System;
using System.Threading.Tasks;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Infrastructure.Interfaces;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
    {
        private bool _isRoot;

        protected virtual void BeginUow(UnitOfWorkInfo uowInfo = null, IMixMediator consumer = null)
        {
            _consumer ??= consumer;
            UowInfo ??= uowInfo;
            if (UowInfo != null)
            {
                _isRoot = false;
                if (UowInfo.ActiveTransaction == null)
                {

                    UowInfo.SetTransaction(
                        UowInfo.ActiveDbContext.Database.CurrentTransaction
                        ?? UowInfo.ActiveDbContext.Database.BeginTransaction());
                }
                Repository ??= new Repository<TDbContext, TEntity, TPrimaryKey>(UowInfo);
                Repository.SetUowInfo(UowInfo);
                return;
            };

            InitRootUow();

        }

        private void InitRootUow()
        {
            _isRoot = true;

            UowInfo = new UnitOfWorkInfo(InitDbContext());
            Repository ??= new Repository<TDbContext, TEntity, TPrimaryKey>(UowInfo);
            Repository.SetUowInfo(UowInfo);
        }

        protected virtual async Task CloseUowAsync()
        {
            if (_isRoot)
            {
                await UowInfo.CloseAsync();
            }
        }

        protected virtual async Task CompleteUowAsync()
        {
            if (_isRoot)
            {
                await UowInfo.CompleteAsync();
                return;
            };

            _isRoot = false;
        }

        private TDbContext InitDbContext()
        {
            var dbContextType = typeof(TDbContext);
            var contextCtorInfo = dbContextType.GetConstructor(new Type[] { });

            if (contextCtorInfo == null)
            {
                HandleException(new MixException(MixErrorStatus.ServerError, $"{dbContextType}: Contructor Parameterless Notfound"));
            }

            return (TDbContext)contextCtorInfo.Invoke(new object[] { });
        }
    }
}
