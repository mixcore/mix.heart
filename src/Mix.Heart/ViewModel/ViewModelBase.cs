using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
    : IViewModel<TPrimaryKey>
      where TPrimaryKey : IComparable
      where TEntity : class, IEntity<TPrimaryKey>
      where TDbContext : DbContext
{
    public UnitOfWorkInfo _unitOfWorkInfo {
        get;
        set;
    }
    public TPrimaryKey Id {
        get;
        set;
    }
    public DateTime CreatedDateTime {
        get;
        set;
    }
    public DateTime? LastModified {
        get;
        set;
    }
    public Guid CreatedBy {
        get;
        set;
    }
    public Guid? ModifiedBy {
        get;
        set;
    }
    public int Priority {
        get;
        set;
    }
    public MixContentStatus Status {
        get;
        set;
    }


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

    public virtual Task ParseView(TEntity entity)
    {
        return Task.Run(() => Mapping(entity));
    }

    public virtual Task<TEntity> ParseEntity<T>(T view)
    where T : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
    {
        var entity = Activator.CreateInstance<TEntity>();
        MapObject(view, entity);
        return Task.FromResult(entity);
    }

    public virtual void Mapping<TSource>(TSource sourceObject)
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
}
}
