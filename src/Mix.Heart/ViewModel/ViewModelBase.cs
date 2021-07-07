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
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        : IViewModel<TPrimaryKey>, IMixMediator
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {
        #region Properties

        public TPrimaryKey Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastModified { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public int Priority { get; set; }
        public MixContentStatus Status { get; set; }
        
        public bool IsValid { get; set; }

        [JsonIgnore]
        protected IMixMediator _consumer;
        [JsonIgnore]
        protected UnitOfWorkInfo UowInfo { get; set; }
        [JsonIgnore]
        public List<ValidationResult> Errors { get; set; } = new List<ValidationResult>();

        #endregion

        #region Contructors

        public ViewModelBase(Repository<TDbContext, TEntity, TPrimaryKey> repository)
        {
            Repository = repository;
        }

        public ViewModelBase(TEntity entity)
        {
            ParseView(entity);
            ExtendView();
        }

        public ViewModelBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            UowInfo = unitOfWorkInfo;
        }

        #endregion

        #region Abstracts

        protected virtual void InitEntityValues() { }
        
        #endregion

        public void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
        {
            Repository.SetUowInfo(unitOfWorkInfo);
        }

        public virtual Task Validate()
        {
            var validateContext = new System.ComponentModel.DataAnnotations.ValidationContext(this, serviceProvider: null, items: null);

            IsValid = Validator.TryValidateObject(this, validateContext, Errors);

            if (!IsValid)
            {
                HandleException(new MixException(MixErrorStatus.Badrequest, Errors.Select(e => e.ErrorMessage).ToArray()));
            }
            return Task.CompletedTask;
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

        protected void HandleErrors()
        {
            HandleException(new MixException(MixErrorStatus.Badrequest, Errors.Select(e => e.ErrorMessage).ToArray()));
        }

        protected virtual Task HandleException(Exception ex)
        {
            return Repository.HandleException(new MixException(MixErrorStatus.ServerError, ex.Message));
        }

        public virtual Task ExtendView()
        {
            return Task.CompletedTask;
        }

        public virtual Task<TEntity> ParseEntity<T>(T view)
            where T : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            InitEntityValues();
            var entity = Activator.CreateInstance<TEntity>();
            MapObject(view, entity);
            return Task.FromResult(entity);
        }

        public virtual void ParseView<TSource>(TSource sourceObject)
            where TSource : TEntity
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(TSource), GetType()));
            var mapper = new Mapper(config);
            mapper.Map(sourceObject, this);
        }

        private void MapObject<TSource, TDestination>(TSource sourceObject, TDestination destObject)
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
    }
}
