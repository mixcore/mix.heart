using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Extensions;
using Mix.Heart.Helpers;
using Mix.Heart.Infrastructure.Exceptions;
using Mix.Heart.Models;
using Mix.Heart.Services;
using Mix.Heart.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public class EntityRepository<TDbContext, TEntity, TPrimaryKey>
        : QueryRepository<TDbContext, TEntity, TPrimaryKey>
        where TPrimaryKey : IComparable
        where TDbContext : DbContext
        where TEntity : class, IEntity<TPrimaryKey>
    {
        public EntityRepository(UnitOfWorkInfo uowInfo) : base(uowInfo) { }
        public EntityRepository(TDbContext dbContext) : base(dbContext) { }

        public EntityRepository()
        {
        }


        #region Async
        public virtual async Task<TEntity> GetEntityByIdAsync(TPrimaryKey id)
        {
            var result = await Table.Where(m => m.Id.Equals(id)).SelectMembers(SelectedMembers).AsNoTracking().SingleOrDefaultAsync();
            if (result != null && CacheService != null)
            {
                if (CacheFilename == "full")
                {
                    await CacheService.SetAsync($"{result.Id}/{typeof(TEntity).FullName}", result, typeof(TEntity), CacheFilename);
                }
                else
                {
                    var obj = ReflectionHelper.GetMembers(result, SelectedMembers);
                    await CacheService.SetAsync($"{result.Id}/{typeof(TEntity).FullName}", obj, typeof(TEntity), CacheFilename);
                }
            }
            return result;
        }

        public virtual async Task<TEntity> GetSingleAsync(TPrimaryKey id)
        {

            if (CacheService != null && CacheService.IsCacheEnabled)
            {
                TEntity result = await CacheService.GetAsync<TEntity>($"{id}/{typeof(TEntity).FullName}", typeof(TEntity), CacheFilename);
                if (result != null)
                {
                    return result;
                }
            }
            return await GetEntityByIdAsync(id);
        }

        protected async Task<List<TEntity>> ParseEntitiesAsync(List<TEntity> entities)
        {
            List<TEntity> data = new List<TEntity>();

            foreach (var entity in entities)
            {
                var view = await GetSingleAsync(entity.Id);
                data.Add(view);
            }
            return data;
        }

        protected async Task<List<TEntity>> GetEntities(IQueryable<TEntity> source)
        {
            return await source.SelectMembers(KeyMembers).ToListAsync();
        }

        protected async Task<PagingResponseModel<TEntity>> ToPagingModelAsync(
           IQueryable<TEntity> source,
           PagingModel pagingData,
           MixCacheService cacheService = null)
        {
            try
            {
                var entities = await GetEntities(source);
                List<TEntity> data = await ParseEntitiesAsync(entities);

                return new PagingResponseModel<TEntity>(data, pagingData);
            }
            catch (Exception ex)
            {
                throw new MixException(ex.Message);
            }
        }

        public virtual async Task<int> MaxAsync(Expression<Func<TEntity, int>> predicate)
        {
            return await GetAllQuery().MaxAsync(predicate);
        }

        public virtual async Task CreateAsync(TEntity entity)
        {
            try
            {
                BeginUow();
                Context.Entry(entity).State = EntityState.Added;
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            try
            {
                BeginUow();

                if (!CheckIsExists(entity))
                {
                    await HandleExceptionAsync(new EntityNotFoundException(entity.Id.ToString()));
                    return;
                }

                Context.Entry(entity).State = EntityState.Modified;
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        public virtual async Task SaveAsync(TEntity entity)
        {
            try
            {
                BeginUow();

                if (CheckIsExists(entity))
                {
                    await UpdateAsync(entity);
                }
                else { await CreateAsync(entity); }
                await CompleteUowAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        public async Task SaveFieldsAsync(TEntity entity, IEnumerable<EntityPropertyModel> properties)
        {
            try
            {
                BeginUow();
                foreach (var property in properties)
                {
                    // check if field name is exist
                    var lamda = ReflectionHelper.GetLambda<TEntity>(property.PropertyName);
                    if (lamda != null)
                    {
                        ReflectionHelper.SetPropertyValue(entity, property);
                    }
                    else
                    {
                        await HandleExceptionAsync(new MixException(MixErrorStatus.Badrequest, $"Invalid Property {property.PropertyName}"));
                    }
                }
                await SaveAsync(entity);
                await CompleteUowAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            try
            {
                BeginUow();
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    await HandleExceptionAsync(new MixException(MixErrorStatus.NotFound, id));
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                BeginUow();
                var entity = await GetSingleAsync(predicate);
                if (entity == null)
                {
                    await HandleExceptionAsync(new EntityNotFoundException());
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        public virtual async Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                BeginUow();

                await Context.Set<TEntity>().Where(predicate).ForEachAsync(
                    m => Context.Entry(m).State = EntityState.Deleted
                    );

                await Context.SaveChangesAsync();
                await CompleteUowAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            try
            {
                BeginUow();

                if (!CheckIsExists(entity))
                {
                    await HandleExceptionAsync(new EntityNotFoundException(entity.Id.ToString()));
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
            }
            finally
            {
                await CloseUowAsync();
            }
        }
        #endregion

        #region IQueryable

        public virtual async Task<PagingResponseModel<TEntity>> GetPagingAsync(
            Expression<Func<TEntity, bool>> predicate,
            PagingModel paging)
        {
            BeginUow();
            var query = GetPagingQuery(predicate, paging);
            return await ToPagingModelAsync(query, paging);
        }


        #endregion

        public void SetSelectedMembers(string[] selectMembers)
        {
            SelectedMembers = selectMembers;
            var properties = typeof(TEntity).GetProperties().Select(p => p.Name);
            var arrIndex = properties
            .Select((prop, index) => new { Property = prop, Index = index })
            .Where(x => selectMembers.Any(m => m.ToLower() == x.Property.ToLower()))
            .Select(x => x.Index.ToString())
            .ToArray();
            CacheFilename = string.Join('-', arrIndex);
        }
    }
}
