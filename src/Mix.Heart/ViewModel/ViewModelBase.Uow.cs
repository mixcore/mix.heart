﻿using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    {
        private bool _isRoot;

        protected virtual void BeginUow()
        {
            if (UowInfo == null)
            {
                InitRootUow();
            }
            UowInfo.Begin();
            Repository ??= new Repository<TDbContext, TEntity, TPrimaryKey, TView>(UowInfo);
            Repository.SetUowInfo(UowInfo);
        }

        public void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
        {
            if (unitOfWorkInfo != null)
            {
                UowInfo = unitOfWorkInfo;
                _isRoot = false;
            }
        }


        protected virtual void InitRootUow()
        {
            UowInfo ??= new UnitOfWorkInfo(InitDbContext());
            _isRoot = true;
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

        protected virtual TDbContext InitDbContext()
        {
            var dbContextType = typeof(TDbContext);
            var contextCtorInfo = dbContextType.GetConstructor(new Type[] { });

            if (contextCtorInfo == null)
            {
                throw new MixException(
                        MixErrorStatus.ServerError,
                        $"{dbContextType}: Contructor Parameterless Notfound");
            }

            return (TDbContext)contextCtorInfo.Invoke(new object[] { });
        }
    }
}
