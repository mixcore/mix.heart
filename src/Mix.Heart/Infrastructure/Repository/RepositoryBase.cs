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
        public UnitOfWorkInfo _unitOfWorkInfo { get; set; }

        public virtual TDbContext Context { get => (TDbContext)_unitOfWorkInfo?.ActiveDbContext; }

        private bool _isRoot;

        protected RepositoryBase(TDbContext dbContext)
        {
            _unitOfWorkInfo = new UnitOfWorkInfo(dbContext);
        }

        public RepositoryBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            _unitOfWorkInfo = unitOfWorkInfo;
        }

        public virtual void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
        {
            if (unitOfWorkInfo != null)
            {
                _isRoot = false;
                _unitOfWorkInfo = unitOfWorkInfo;
            };
        }

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

            InitRootUow();

        }

        private void InitRootUow()
        {
            _isRoot = true;
            var context = InitDbContext();
            _unitOfWorkInfo = new UnitOfWorkInfo(context);
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
            if (_isRoot)
            {
                _unitOfWorkInfo.Close();
            }
        }

        protected virtual async Task CloseUowAsync()
        {
            if (_isRoot)
            {
                await _unitOfWorkInfo.CloseAsync();
            }
        }

        protected virtual async Task CompleteUowAsync()
        {
            if (_isRoot)
            {
                await _unitOfWorkInfo.CompleteAsync();
                _unitOfWorkInfo.Close();
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
                throw new NullReferenceException();
            }

            return (TDbContext)contextCtorInfo.Invoke(new object[] { });
        }

        protected void HandleException(Exception ex)
        {
            throw new MixHttpResponseException(MixErrorStatus.Badrequest, ex.Message);
        }


    }
}
