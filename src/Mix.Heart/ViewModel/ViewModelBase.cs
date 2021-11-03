using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Infrastructure.Interfaces;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
        : IViewModel, IMixMediator
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

        public bool IsValid { get; set; }

        [JsonIgnore]
        protected IMixMediator _consumer;
        [JsonIgnore]
        protected UnitOfWorkInfo UowInfo { get; set; }
        [JsonIgnore]
        public List<ValidationResult> Errors { get; set; } = new List<ValidationResult>();

        protected Repository<TDbContext, TEntity, TPrimaryKey, TView> Repository { get; set; }
        protected TDbContext Context { get => (TDbContext)UowInfo?.ActiveDbContext; }

        #endregion

        #region Contructors

        public ViewModelBase()
        {

        }
        
        public ViewModelBase(TDbContext context)
        {
            UowInfo = new UnitOfWorkInfo(context);
            _isRoot = true;
        }

        public ViewModelBase(TEntity entity, UnitOfWorkInfo uowInfo = null)
        {
            SetUowInfo(uowInfo);
            ParseView(entity);
            ExpandView(UowInfo);
        }

        public ViewModelBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            UowInfo = unitOfWorkInfo;
        }

        #endregion

        #region Abstracts

        public virtual void InitDefaultValues(string language = null, int? cultureId = null)
        {
            CreatedDateTime = DateTime.UtcNow;
            Status = MixContentStatus.Published;
        }

        #endregion

        // use for public
        public static Repository<TDbContext, TEntity, TPrimaryKey, TView> GetRepository(UnitOfWorkInfo uowInfo)
        {
            return new Repository<TDbContext, TEntity, TPrimaryKey, TView>(uowInfo);
        }

        // use for public
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

        public void SetConsumer(IMixMediator consumer)
        {
            _consumer = consumer;
        }

        public virtual TEntity InitModel()
        {
            Type classType = typeof(TEntity);
            return (TEntity)Activator.CreateInstance(classType);
        }

        public virtual Task ExpandView(UnitOfWorkInfo uowInfo)
        {
            return Task.CompletedTask;
        }

        public virtual Task<TEntity> ParseEntity()
        {
            if (IsDefaultId(Id))
            {
                InitDefaultValues();
            }
            var entity = Activator.CreateInstance<TEntity>();
            MapObject(this, entity);
            return Task.FromResult(entity);
        }

        public virtual void ParseView<TSource>(TSource sourceObject)
            where TSource : TEntity
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(TSource), GetType()));
            var mapper = new Mapper(config);
            mapper.Map(sourceObject, this);
        }

        public Task PublishAsync(object sender, MixViewModelAction action, bool isSucceed, Exception ex = null)
        {
            return _consumer != null
               ? _consumer.ConsumeAsync(sender, action, isSucceed)
               : Task.CompletedTask;
        }

        public virtual Task ConsumeAsync(object sender, MixViewModelAction action, bool isSucceed, Exception ex = null)
        {
            return PublishAsync(sender, action, isSucceed, ex);
        }

        protected bool IsDefaultId(TPrimaryKey id)
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

        protected void MapObject<TSource, TDestination>(TSource sourceObject, TDestination destObject)
            where TSource : class
            where TDestination : class
        {
            var sourceType = sourceObject.GetType();
            var destType = destObject.GetType();

            var sourceProperties = sourceType.GetProperties();
            var destProperties = destType.GetProperties();

            var commonProperties =
                from sourceProperty in sourceProperties
                join destProperty in destProperties
                    on new
                    {
                        sourceProperty.Name,
                        sourceProperty.PropertyType
                    }
                    equals new
                    {
                        destProperty.Name,
                        destProperty.PropertyType
                    }
                select new
                {
                    SourceProperty = sourceProperty,
                    DestProperty = destProperty
                };

            foreach (var property in commonProperties)
            {
                property.DestProperty.SetValue(destObject, property.SourceProperty.GetValue(sourceObject));
            }
        }
    }
}
