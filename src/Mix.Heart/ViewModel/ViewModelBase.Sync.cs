using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entity;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TPrimaryKey, TEntity, TDbContext>
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {
        public virtual void Save(bool hasSavedRelationship = false, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(ref uowInfo);

                var context = uowInfo.ActiveDbContext;

                var entity = context.Set<TEntity>().Find(Id);
                if (entity != null)
                {
                    MapObject(this, entity);
                    context.Update(entity);
                }
                else
                {
                    entity = Activator.CreateInstance<TEntity>();
                    MapObject(this, entity);

                    context.Add(entity);
                    context.SaveChanges();
                }

                if (hasSavedRelationship)
                {
                    SaveEntityRelationship(entity, uowInfo);
                }
            }
            catch (Exception ex)
            {
                if (!_isRoot)
                {
                    throw;
                };

                CloseUow(uowInfo);
                Console.WriteLine(ex.Message);
            }
            finally
            {
                CompleteUow(uowInfo);
            }
        }

        protected virtual void SaveEntityRelationship(TEntity parentEntity, UnitOfWorkInfo uowInfo)
        {
            throw new NotImplementedException();
        }

        protected virtual void MapObject<TSource, TDestination>(TSource sourceObject, TDestination destObject)
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
