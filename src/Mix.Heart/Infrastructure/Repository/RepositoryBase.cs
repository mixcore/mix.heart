using Microsoft.EntityFrameworkCore;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public abstract class RepositoryBase<TDbContext> : IRepositoryBase<TDbContext> where TDbContext : DbContext
    {
        public UnitOfWorkInfo UowInfo { get; set; }

        public virtual TDbContext Context { get => (TDbContext)UowInfo?.ActiveDbContext; }

        private bool _isRoot;

        protected RepositoryBase(TDbContext dbContext)
        {
            UowInfo = new UnitOfWorkInfo(dbContext);
            _isRoot = true;
        }
        public RepositoryBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            UowInfo = unitOfWorkInfo;
            _isRoot = false;
        }

        public virtual void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
        {
            if (unitOfWorkInfo != null)
            {
                _isRoot = false;
                UowInfo = unitOfWorkInfo;
            };
        }

        protected virtual void BeginUow(UnitOfWorkInfo uowInfo = null)
        {
            UowInfo ??= uowInfo;
            if (uowInfo != null)
            {
                _isRoot = false;
            };

            if (UowInfo != null)
            {
                if (UowInfo.ActiveTransaction == null)
                {

                    UowInfo.SetTransaction(
                        UowInfo.ActiveDbContext.Database.CurrentTransaction
                        ?? UowInfo.ActiveDbContext.Database.BeginTransaction());
                }
                return;
            }

            InitRootUow();

        }

        private void InitRootUow()
        {
            _isRoot = true;
            var context = InitDbContext();
            UowInfo = new UnitOfWorkInfo(context);
          
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
                UowInfo.Close();
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

        public Task HandleException(Exception ex)
        {
            throw new MixException(MixErrorStatus.Badrequest, ex);
        }


    }
}
