using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Extensions;
using Mix.Heart.Helpers;
using Mix.Heart.Model;
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
    public class QueryRepository<TDbContext, TEntity, TPrimaryKey>
        : RepositoryBase<TDbContext>
        where TDbContext : DbContext
        where TEntity : class, IEntity<TPrimaryKey>
        where TPrimaryKey : IComparable
    {
        public QueryRepository(TDbContext dbContext) : base(dbContext) { }

        public QueryRepository()
        {
        }

        public QueryRepository(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
        {
        }


        #region IQueryable

        public IQueryable<TEntity> GetAllQuery()
        {
            return Context.Set<TEntity>().AsQueryable().AsNoTracking();
        }

        public IQueryable<TEntity> GetListQuery(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAllQuery().Where(predicate);
        }

        public IQueryable<TEntity> GetPagingQuery(Expression<Func<TEntity, bool>> predicate,
                       IPagingModel paging)
        {
            var query = GetListQuery(predicate);
            paging.Total = query.Count();
            dynamic sortBy = GetLambda(paging.SortBy);

            switch (paging.SortDirection)
            {
                case Enums.SortDirection.Asc:
                    query = Queryable.OrderBy(query, sortBy);
                    break;
                case Enums.SortDirection.Desc:
                    query = Queryable.OrderByDescending(query, sortBy);
                    break;
            }

            if (paging.PageSize.HasValue)
            {
                query = query.Skip(paging.PageIndex * paging.PageSize.Value).Take(paging.PageSize.Value);
            }

            return query;
        }

        #endregion

        public virtual bool CheckIsExists(TEntity entity)
        {
            return GetAllQuery().Any(e => e.Id.Equals(entity.Id));
        }

        public virtual bool CheckIsExists(Func<TEntity, bool> predicate)
        {
            return GetAllQuery().Any(predicate);
        }

        #region Entity Async

        public async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await GetAllQuery().SingleOrDefaultAsync(predicate);
        }

        public virtual async Task<TEntity> GetByIdAsync(TPrimaryKey id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }

        public virtual int MaxAsync(Func<TEntity, int> predicate)
        {
            return GetAllQuery().Max(predicate);
        }

        public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAllQuery().FirstOrDefaultAsync(predicate);
        }

        #endregion

        #region Entity Sync

        public TEntity GetSingle(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return GetAllQuery().SingleOrDefault(predicate);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return default;
            }
        }

        public virtual TEntity GetById(TPrimaryKey id)
        {
            try
            {
                return Context.Set<TEntity>().Find(id);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return default;
            }
        }

        public virtual int Max(Func<TEntity, int> predicate)
        {
            try
            {
                return GetAllQuery().Max(predicate);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return default;
            }
        }

        public TEntity GetFirst(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return GetAllQuery().FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return default;
            }
        }

        #endregion

        #region View Async

        public virtual async Task<TView> GetSingleViewAsync<TView>(TPrimaryKey id)
            where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                return await BuildViewModel<TView>(entity);
            }
            throw new MixException(MixErrorStatus.NotFound, id);
        }

        public virtual async Task<TView> GetSingleViewAsync<TView>(Expression<Func<TEntity, bool>> predicate)
            where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            var entity = await GetSingleAsync(predicate);
            if (entity != null)
            {
                var result = await BuildViewModel<TView>(entity);
                return result;
            }
            return null;
        }

        public virtual async Task<List<TView>> GetListViewAsync<TView>(
                Expression<Func<TEntity, bool>> predicate, UnitOfWorkInfo uowInfo = null)
            where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            BeginUow(uowInfo);
            var query = GetListQuery(predicate);
            var result = await ToListViewModelAsync<TView>(query);
            await CloseUowAsync();
            return result;
        }

        public virtual async Task<PagingResponseModel<TView>> GetPagingViewAsync<TView>(
            Expression<Func<TEntity, bool>> predicate, IPagingModel paging, UnitOfWorkInfo uowInfo = null)
            where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            BeginUow(uowInfo);
            var query = GetPagingQuery(predicate, paging);
            return await ToPagingViewModelAsync<TView>(query, paging);
        }

        #endregion



        #region Helper
        #region Private methods

        protected virtual Task<TView> BuildViewModel<TView>(TEntity entity, UnitOfWorkInfo uowInfo = null)
            where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            ConstructorInfo classConstructor = typeof(TView).GetConstructor(new Type[] { typeof(TEntity), typeof(UnitOfWorkInfo) });
            return Task.FromResult((TView)classConstructor.Invoke(new object[] { entity, uowInfo }));
        }

        public async Task<List<TView>> ToListViewModelAsync<TView>(
           IQueryable<TEntity> source,
           bool isCache = false)
            where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            try
            {
                var keys = ReflectionHelper.GetKeyMembers(Context, typeof(TEntity));
                var members = isCache ? keys
                                        : ReflectionHelper.FilterSelectedFields<TView, TEntity>();
                var entities = await source.SelectMembers(members).ToListAsync();

                List<TView> data = new List<TView>();
                ConstructorInfo classConstructor = typeof(TView).GetConstructor(new Type[] { typeof(TEntity), typeof(UnitOfWorkInfo) });
                foreach (var entity in entities)
                {
                    var view = await BuildViewModel<TView>(entity, UowInfo);
                    data.Add(view);
                }

                // TODO: Handle cache service
                //if (isCache)
                //{
                //    var lstView = GetListCachedData(entities, repository, keys);
                //    result.Items = lstView;
                //}
                //else
                //{
                //    result.Items = ParseView(lsTEntity, context, transaction);
                //}

                return data;
            }
            catch (Exception ex)
            {
                throw new MixException(ex.Message);
            }
        }

        protected async Task<PagingResponseModel<TView>> ToPagingViewModelAsync<TView>(
            IQueryable<TEntity> source,
            IPagingModel pagingData,
            bool isCache = false)
            where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            try
            {
                var keys = ReflectionHelper.GetKeyMembers(Context, typeof(TEntity));
                var members = isCache ? keys
                                        : ReflectionHelper.FilterSelectedFields<TView, TEntity>();
                var entities = await source.SelectMembers(members).ToListAsync();

                List<TView> data = new List<TView>();
                ConstructorInfo classConstructor = typeof(TView).GetConstructor(new Type[] { typeof(TEntity), typeof(UnitOfWorkInfo) });
                foreach (var entity in entities)
                {
                    var view = (TView)classConstructor.Invoke(new object[] { entity, UowInfo });
                    data.Add(view);
                }

                // TODO: Handle cache service
                //if (isCache)
                //{
                //    var lstView = GetListCachedData(entities, repository, keys);
                //    result.Items = lstView;
                //}
                //else
                //{
                //    result.Items = ParseView(lsTEntity, context, transaction);
                //}

                return new PagingResponseModel<TView>(data, pagingData);
            }
            catch (Exception ex)
            {
                throw new MixException(ex.Message);
            }
        }

        private List<TView> GetListCachedData<TView>(
            List<TEntity> entities,
            QueryRepository<TDbContext, TEntity, TPrimaryKey> repository,
            params string[] keys)
            where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        {
            List<TView> result = new List<TView>();
            foreach (var entity in entities)
            {
                TView data = (TView)GetCachedData(entity, repository, keys);
                if (data != null)
                {
                    result.Add(data);
                }
            }
            return result;
        }

        private IViewModel GetCachedData(
            TEntity entity, QueryRepository<TDbContext, TEntity, TPrimaryKey> repository, string[] keys)
        {
            throw new NotImplementedException();
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
