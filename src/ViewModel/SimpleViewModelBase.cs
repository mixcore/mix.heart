using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Helpers;
using Mix.Heart.UnitOfWork;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public abstract partial class SimpleViewModelBase<TDbContext, TEntity, TPrimaryKey, TView> : ViewModelQueryBase<TDbContext, TEntity, TPrimaryKey, TView>
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
        where TView : SimpleViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    {
        #region Properties

        public TPrimaryKey Id { get; set; }

        #endregion

        #region Constructors

        public SimpleViewModelBase() : base()
        {
        }

        public SimpleViewModelBase(TDbContext context) : base(context)
        {
        }

        public SimpleViewModelBase(TEntity entity, UnitOfWorkInfo uowInfo) : base(entity, uowInfo)
        {
        }

        public SimpleViewModelBase(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
        {
        }

        #endregion

        #region Abstracts
        public virtual void InitDefaultValues(string language = null, int? cultureId = null)
        {
        }

        #endregion

        public virtual async Task Validate(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsValid)
            {
                await HandleExceptionAsync(new MixException(MixErrorStatus.Badrequest, Errors.Select(e => e.ErrorMessage).ToArray()));
            }
        }

        public void SetDbContext(TDbContext context)
        {
            UowInfo = new UnitOfWorkInfo(context);
        }

        public virtual TEntity InitModel()
        {
            Type classType = typeof(TEntity);
            return (TEntity)Activator.CreateInstance(classType);
        }

        public virtual Task<TEntity> ParseEntity(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsDefaultId(Id))
            {
                InitDefaultValues();
            }

            var entity = Activator.CreateInstance<TEntity>();
            ReflectionHelper.Map(this as TView, entity);
            return Task.FromResult(entity);
        }

        public bool IsDefaultId(TPrimaryKey id)
        {
            return (id.GetType() == typeof(Guid) && Guid.Parse(id.ToString()) == Guid.Empty)
                || (id.GetType() == typeof(int) && int.Parse(id.ToString()) == default);
        }

        public virtual Task DuplicateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public virtual void Duplicate()
        {
        }

        protected async Task HandleErrorsAsync()
        {
            await HandleExceptionAsync(new MixException(MixErrorStatus.Badrequest, Errors.Select(e => e.ErrorMessage).ToArray()));
        }

        protected virtual async Task HandleExceptionAsync(Exception ex)
        {
            await Repository.HandleExceptionAsync(ex);
        }

        protected virtual void HandleException(Exception ex)
        {
            Repository.HandleException(ex);
        }
    }
}
