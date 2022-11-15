using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
        : IViewModel
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
        where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    {
        #region Properties

        public TPrimaryKey Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastModified { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int Priority { get; set; }
        public MixContentStatus Status { get; set; } = MixContentStatus.Published;

        [JsonIgnore]
        public bool IsValid { get; set; }

        [JsonIgnore]
        protected UnitOfWorkInfo UowInfo { get; set; }
        [JsonIgnore]
        public List<ValidationResult> Errors { get; set; } = new List<ValidationResult>();
        [JsonIgnore]
        protected Repository<TDbContext, TEntity, TPrimaryKey, TView> Repository { get; set; }
        protected TDbContext Context { get => (TDbContext)UowInfo?.ActiveDbContext; }

        #endregion

        #region Constructors

        public ViewModelBase()
        {
            Repository ??= GetRepository(UowInfo);
        }

        public ViewModelBase(TDbContext context)
        {
            UowInfo = new UnitOfWorkInfo(context);
            Repository ??= GetRepository(UowInfo);
            _isRoot = true;
        }

        public ViewModelBase(TEntity entity, UnitOfWorkInfo uowInfo = null)
        {
            SetUowInfo(uowInfo);
            ParseView(entity);
        }

        public ViewModelBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            SetUowInfo(unitOfWorkInfo);
        }

        #endregion

        #region Abstracts

        public virtual void InitDefaultValues(string language = null, int? cultureId = null)
        {
            CreatedDateTime = DateTime.UtcNow;
            Status = MixContentStatus.Published;
        }

        #endregion

        public virtual Task ExpandView(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public static Repository<TDbContext, TEntity, TPrimaryKey, TView> GetRepository(UnitOfWorkInfo uowInfo)
        {
            return new Repository<TDbContext, TEntity, TPrimaryKey, TView>(uowInfo);
        }

        public static Repository<TDbContext, TEntity, TPrimaryKey, TView> GetRootRepository(TDbContext context)
        {
            return new Repository<TDbContext, TEntity, TPrimaryKey, TView>(context);
        }

        public virtual async Task Validate()
        {
            var validateContext = new System.ComponentModel.DataAnnotations.ValidationContext(this, serviceProvider: null, items: null);

            IsValid = Validator.TryValidateObject(this, validateContext, Errors);

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



        public virtual Task<TEntity> ParseEntity()
        {
            if (IsDefaultId(Id))
            {
                InitDefaultValues();
            }
            var entity = Activator.CreateInstance<TEntity>();
            var config = new MapperConfiguration(cfg => cfg.CreateMap(GetType(), typeof(TEntity)));
            var mapper = new Mapper(config);
            mapper.Map(this, entity);
            return Task.FromResult(entity);
        }

        public virtual void ParseView<TSource>(TSource sourceObject)
            where TSource : TEntity
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(TSource), GetType()));
            var mapper = new Mapper(config);
            mapper.Map(sourceObject, this);
        }

        public bool IsDefaultId(TPrimaryKey id)
        {
            return (id.GetType() == typeof(Guid) && Guid.Parse(id.ToString()) == Guid.Empty)
                || (id.GetType() == typeof(int) && int.Parse(id.ToString()) == default);
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
