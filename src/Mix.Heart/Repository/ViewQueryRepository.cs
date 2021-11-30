using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Extensions;
using Mix.Heart.Helpers;
using Mix.Heart.Model;
using Mix.Heart.Services;
using Mix.Heart.UnitOfWork;
using Mix.Heart.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public class ViewQueryRepository<TDbContext, TEntity, TPrimaryKey, TView>
        : RepositoryBase<TDbContext>
        where TDbContext : DbContext
        where TEntity : class, IEntity<TPrimaryKey>
        where TPrimaryKey : IComparable
        where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    {

        public ViewQueryRepository(TDbContext dbContext) : base(dbContext) { }

        public ViewQueryRepository(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
        {
        }

        protected string[] SelectedMembers { get { return FilterSelectedFields(); } }
        protected string[] KeyMembers { get { return ReflectionHelper.GetKeyMembers(Context, typeof(TEntity)); } }

        protected DbSet<TEntity> Table => Context.Set<TEntity>();

        #region IQueryable

        public IQueryable<TEntity> GetListQuery(Expression<Func<TEntity, bool>> predicate)
        {
            return Table.Where(predicate);
        }

        public IQueryable<TEntity> GetPagingQuery(Expression<Func<TEntity, bool>> predicate,
                       IPagingModel paging)
        {
            var query = GetListQuery(predicate);
            paging.Total = query.Count();
            dynamic sortBy = GetLambda(paging.SortBy);

            switch (paging.SortDirection)
            {
                case SortDirection.Asc:
                    query = Queryable.OrderBy(query, sortBy);
                    break;
                case SortDirection.Desc:
                    query = Queryable.OrderByDescending(query, sortBy);
                    break;
            }

            if (paging.PageSize.HasValue)
            {
                query = query.Skip(paging.PageIndex * paging.PageSize.Value).Take(paging.PageSize.Value);
            }

            return query;
        }

        public virtual async Task<TEntity> GetByIdAsync(TPrimaryKey id)
        {
            return await Table.SelectMembers(SelectedMembers).AsNoTracking().SingleOrDefaultAsync(m => m.Id.Equals(id));
        }
        #endregion

        public virtual async Task<bool> CheckIsExistsAsync(TEntity entity)
        {
            return await Table.AnyAsync(e => e.Id.Equals(entity.Id));
        }

        public virtual async Task<bool> CheckIsExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Table.AnyAsync(predicate);
        }

        #region View Async

        public virtual async Task<TView> GetSingleAsync(TPrimaryKey id, MixCacheService cacheService = null)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                return await ParseEntityAsync(entity, cacheService);
            }
            throw new MixException(MixErrorStatus.NotFound, id);
        }

        public virtual async Task<TView> GetSingleAsync(Expression<Func<TEntity, bool>> predicate, MixCacheService cacheService = null)
        {
            var entity = await Table.AsNoTracking().SingleOrDefaultAsync(predicate);
            if (entity != null)
            {
                return await ParseEntityAsync(entity, cacheService);
            }
            return null;
        }

        public virtual async Task<List<TView>> GetListAsync(
                Expression<Func<TEntity, bool>> predicate,
                MixCacheService cacheService = null,
                UnitOfWorkInfo uowInfo = null)
        {
            var query = GetListQuery(predicate);
            var result = await ToListViewModelAsync(query, cacheService);
            return result;
        }

        public virtual async Task<PagingResponseModel<TView>> GetPagingAsync(
            Expression<Func<TEntity, bool>> predicate,
            IPagingModel paging,
            MixCacheService cacheService = null,
            UnitOfWorkInfo uowInfo = null)
        {
            BeginUow(uowInfo);
            var query = GetPagingQuery(predicate, paging);
            return await ToPagingViewModelAsync(query, paging, cacheService);
        }

        #endregion



        #region Helper
        #region Private methods

        private string[] FilterSelectedFields()
        {
            var viewProperties = typeof(TView).GetProperties();
            var modelProperties = typeof(TEntity).GetProperties();
            return viewProperties.Where(p => modelProperties.Any(m => m.Name == p.Name)).Select(p => p.Name).ToArray();
        }

        protected virtual Task<TView> BuildViewModel(TEntity entity, UnitOfWorkInfo uowInfo = null)
        {
            ConstructorInfo classConstructor = typeof(TView).GetConstructor(new Type[] { typeof(TEntity), typeof(UnitOfWorkInfo) });
            return Task.FromResult((TView)classConstructor.Invoke(new object[] { entity, uowInfo }));
        }

        public async Task<List<TView>> ToListViewModelAsync(
           IQueryable<TEntity> source,
            MixCacheService cacheService = null)
        {
            try
            {
                var entities = await source.SelectMembers(SelectedMembers).AsNoTracking().ToListAsync();

                List<TView> data = await ParseEntitiesAsync(entities, cacheService);

                return data;
            }
            catch (Exception ex)
            {
                throw new MixException(ex.Message);
            }
        }

        protected async Task<PagingResponseModel<TView>> ToPagingViewModelAsync(
            IQueryable<TEntity> source,
            IPagingModel pagingData,
            MixCacheService cacheService = null)
        {
            try
            {
                var entities = await GetEntities(source);
                List<TView> data = await ParseEntitiesAsync(entities, cacheService);

                return new PagingResponseModel<TView>(data, pagingData);
            }
            catch (Exception ex)
            {
                throw new MixException(ex.Message);
            }
        }

        protected async Task<List<TEntity>> GetEntities(IQueryable<TEntity> source)
        {
            return await source.SelectMembers(SelectedMembers).ToListAsync();
        }



        protected async Task<List<TView>> ParseEntitiesAsync(List<TEntity> entities, MixCacheService cacheService = null)
        {
            List<TView> data = new List<TView>();

            foreach (var entity in entities)
            {
                var view = await ParseEntityAsync(entity, cacheService);
                data.Add(view);
            }
            return data;
        }

        protected async Task<TView> ParseEntityAsync(TEntity entity, MixCacheService cacheService = null)
        {
            TView result = null;
            if (cacheService != null && cacheService.IsCacheEnabled)
            {
                result = await cacheService.GetAsync<TView>(entity.Id.ToString(), typeof(TView));
            }

            if (result == null)
            {
                result = GetViewModel(entity, cacheService);

                if (result != null && cacheService != null)
                {
                    await cacheService.SetAsync(entity.Id.ToString(), result, typeof(TView));
                }
            }
            await result.ExpandView(cacheService, UowInfo);
            return result;

        }

        protected TView GetViewModel(TEntity entity, MixCacheService cacheService = null)
        {
            ConstructorInfo classConstructor = typeof(TView).GetConstructor(
                new Type[] { typeof(TEntity), typeof(MixCacheService), typeof(UnitOfWorkInfo) });
            return (TView)classConstructor.Invoke(new object[] { entity, cacheService, UowInfo });
        }

        #endregion

        protected LambdaExpression GetLambda(string propName,
                                             bool isGetDefault = true)
        {
            var parameter = Expression.Parameter(typeof(TEntity));
            var type = typeof(TEntity);
            var prop = Array.Find(type.GetProperties(), p => p.Name == propName);
            if (prop == null && isGetDefault)
            {
                propName = "Id";
            }
            var memberExpression = Expression.Property(parameter, propName);
            return Expression.Lambda(memberExpression, parameter);
        }

        #endregion
    }
}
