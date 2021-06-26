using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mix.Heart.UnitOfWork;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
    {
        public virtual void Save(bool hasSavedRelationship = false, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(uowInfo);

                var context = _unitOfWorkInfo.ActiveDbContext;

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
                    SaveEntityRelationship(entity);
                }
            }
            catch (Exception ex)
            {
                if (!_isRoot)
                {
                    throw;
                };

                CloseUow();
                Console.WriteLine(ex.Message);
            }
            finally
            {
                CompleteUow();
            }
        }

        protected virtual Task SaveEntityRelationship(TEntity parentEntity)
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
