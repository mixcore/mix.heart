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
        public UnitOfWorkInfo UnitOfWorkInfo { get; set; }

        public virtual TDbContext Context => (TDbContext)(UnitOfWorkInfo?.ActiveDbContext);

        private bool _isRoot;

        public RepositoryBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            UnitOfWorkInfo = unitOfWorkInfo;
        }

        protected RepositoryBase(TDbContext dbContext)
        {
        }

        public virtual void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
        {
            if (unitOfWorkInfo != null)
            {
                _isRoot = false;
                UnitOfWorkInfo = unitOfWorkInfo;
            };
        }

        protected virtual void BeginUow(UnitOfWorkInfo uowInfo = null)
        {
            UnitOfWorkInfo ??= uowInfo;
            if (UnitOfWorkInfo != null)
            {
                _isRoot = false;
                if (UnitOfWorkInfo.ActiveTransaction == null)
                {

                    UnitOfWorkInfo.SetTransaction(
                        UnitOfWorkInfo.ActiveDbContext.Database.CurrentTransaction
                        ?? UnitOfWorkInfo.ActiveDbContext.Database.BeginTransaction());
                }
                return;
            };

            InitRootUow();

        }

        private void InitRootUow()
        {
            _isRoot = true;
            var dbContext = InitDbContext();
            UnitOfWorkInfo = new UnitOfWorkInfo(dbContext);
        }

        protected virtual void CompleteUow()
        {
            if (!_isRoot)
            {
                return;
            };

            UnitOfWorkInfo.Complete();

            _isRoot = false;

            Console.WriteLine("Unit of work completed.");
        }

        protected virtual void CloseUow()
        {
            UnitOfWorkInfo.Close();
        }

        protected virtual async Task CompleteUowAsync()
        {
            if (!_isRoot)
            {
                return;
            };

            await UnitOfWorkInfo.CompleteAsync();
            UnitOfWorkInfo.Close();

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

        protected void HandleException(Exception ex)
        {
            CloseUow();
            throw new MixHttpResponseException(MixErrorStatus.Badrequest, ex.Message);
        }


    }
}
