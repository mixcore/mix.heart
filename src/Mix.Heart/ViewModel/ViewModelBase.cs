using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entity;
using Mix.Heart.Enums;
using Mix.Heart.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        : IViewModel<TPrimaryKey>
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {
        public UnitOfWorkInfo _unitOfWorkInfo { get; set; }
        public TPrimaryKey Id { get; set; }
        public DateTime CreatedDateTime { get; set;}
        public DateTime? LastModified { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public int Priority { get; set; }
        public MixContentStatus Status { get; set;}


        public virtual TEntity InitModel()
        {
            Type classType = typeof(TEntity);
            return (TEntity)Activator.CreateInstance(classType);
        }

        protected void HandleException(Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }

        public virtual Task<T> ParseView<T>(TEntity entity)
            where T : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            var view = Activator.CreateInstance<T>();
            MapObject(entity, view);
            return Task.FromResult(view);
        }

        public virtual Task<TEntity> ParseEntity<T>(T view)
            where T : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            var entity = Activator.CreateInstance<TEntity>();
            MapObject(view, entity);
            return Task.FromResult(entity);
        }

        protected virtual void Mapping<TSource, TDestination>(TSource sourceObject, TDestination destObject)
            where TSource : class
            where TDestination : class
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(TSource), typeof(TDestination)));
            var mapper = new Mapper(config);
            mapper.Map(sourceObject, destObject);
        }
    }
}
