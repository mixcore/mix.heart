﻿using Microsoft.EntityFrameworkCore;
using Mix.Heart.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public abstract class RepositoryBase<TDbContext> : IRepositoryBase<TDbContext> where TDbContext : DbContext
    {
        public TDbContext _dbContext { get; private set; }
        public UnitOfWorkInfo _unitOfWorkInfo { get; set; }

        public RepositoryBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            _unitOfWorkInfo = unitOfWorkInfo;
        }
        protected RepositoryBase(TDbContext dbContext)
        {
            _isRoot = true;
            _dbContext = dbContext;
            _unitOfWorkInfo ??= new UnitOfWorkInfo();
            _unitOfWorkInfo.SetDbContext(_dbContext);
        }

        private bool _isRoot;

        public virtual void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
        {
            if (unitOfWorkInfo != null)
            {
                _isRoot = false;
                _unitOfWorkInfo = unitOfWorkInfo;
                _dbContext = (TDbContext)unitOfWorkInfo.ActiveDbContext;
            };
        }

        protected virtual void BeginUow()
        {
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

            _isRoot = true;

            var dbContextType = typeof(TDbContext);
            var contextCtorInfo = dbContextType.GetConstructor(new Type[] { });

            if (contextCtorInfo == null)
            {
                throw new NullReferenceException();
            }

            var dbContext = (TDbContext)contextCtorInfo.Invoke(new object[] { });

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

        public virtual TDbContext Context => _dbContext;

        protected void HandleException(Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }
    }
}
