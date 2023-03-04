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

        protected ValidationContext ValidateContext;
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public static bool IsCache { get; set; } = true;
        [JsonIgnore]
        public static string CacheFolder { get; set; } = typeof(TEntity).FullName;
        [JsonIgnore]
        protected bool IsValid { get; set; }

        [JsonIgnore]
        protected UnitOfWorkInfo UowInfo { get; set; }
        [JsonIgnore]
        protected List<ValidationResult> Errors { get; set; } = new List<ValidationResult>();
        [JsonIgnore]
        protected Repository<TDbContext, TEntity, TPrimaryKey, TView> Repository { get; set; }
        protected TDbContext Context { get => (TDbContext)UowInfo?.ActiveDbContext; }

        #endregion

        #region Constructors

        public ViewModelBase()
        {
            ValidateContext = new ValidationContext(this, serviceProvider: null, items: null);
            Repository ??= GetRepository(UowInfo);
        }

        public ViewModelBase(TDbContext context)
        {
            ValidateContext = new ValidationContext(this, serviceProvider: null, items: null);
            UowInfo = new UnitOfWorkInfo(context);
            Repository ??= GetRepository(UowInfo);

            _isRoot = true;
        }

        public ViewModelBase(TEntity entity, UnitOfWorkInfo? uowInfo)
        {
            ValidateContext = new ValidationContext(this, serviceProvider: null, items: null);
            SetUowInfo(uowInfo);
            ParseView(entity);
        }

        public ViewModelBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            ValidateContext = new ValidationContext(this, serviceProvider: null, items: null);
            SetUowInfo(unitOfWorkInfo);
        }

        #endregion

        #region Abstracts

        public virtual void InitDefaultValues(string language = null, int? cultureId = null)
        {
            CreatedDateTime = DateTime.UtcNow;
            Status = MixContentStatus.Published;
            IsDeleted = false;
        }

        #endregion

        public virtual Task ExpandView(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public static Repository<TDbContext, TEntity, TPrimaryKey, TView> GetRepository(UnitOfWorkInfo uowInfo, bool isCache = true, string cacheFolder = null)
        {
            return new Repository<TDbContext, TEntity, TPrimaryKey, TView>(uowInfo)
            {
                IsCache = isCache,
                CacheFolder = cacheFolder ?? CacheFolder
            };
        }

        public static Repository<TDbContext, TEntity, TPrimaryKey, TView> GetRootRepository(TDbContext context)
        {
            return new Repository<TDbContext, TEntity, TPrimaryKey, TView>(context)
            {
                CacheFolder = CacheFolder
            };
        }

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
            var config = new MapperConfiguration(cfg => cfg.CreateMap(GetType(), typeof(TEntity)));
            var mapper = new Mapper(config);
            mapper.Map(this, entity);
            return Task.FromResult(entity);
        }

        public virtual void ParseView<TSource>(TSource sourceObject, CancellationToken cancellationToken = default)
            where TSource : TEntity
        {
            cancellationToken.ThrowIfCancellationRequested();
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
