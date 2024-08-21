using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Extensions;
using Mix.Heart.Helpers;
using Mix.Heart.Models;
using Mix.Heart.Services;
using Mix.Heart.UnitOfWork;
using Mix.Heart.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
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
        public bool IsCache { get; set; } = true;
        public ViewQueryRepository(TDbContext dbContext) : base(dbContext)
        {
            SelectedMembers = typeof(TView).GetProperties().Select(m => m.Name).ToArray();
        }

        public ViewQueryRepository(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
        {
            SelectedMembers = typeof(TView).GetProperties().Select(m => m.Name).ToArray();
        }

        public MixCacheService CacheService { get; set; }

        public string CacheFilename { get; private set; } = "full";

        public string CacheFolder { get; set; } = typeof(TEntity).FullName;

        public string[] SelectedMembers { get; private set; }

        protected string[] KeyMembers { get { return ReflectionHelper.GetKeyMembers(Context, typeof(TEntity)); } }

        public DbSet<TEntity> Table => Context?.Set<TEntity>();

        #region IQueryable

        public IQueryable<TEntity> GetListQuery(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Table.AsNoTracking().Where(predicate);
        }

        public IQueryable<TEntity> GetSortedQuery(
            Expression<Func<TEntity, bool>> predicate,
            string sortByFieldName, SortDirection direction,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = GetListQuery(predicate, cancellationToken);
            dynamic sortBy = GetLambda(sortByFieldName);

            switch (direction)
            {
                case SortDirection.Asc:
                    query = Queryable.OrderBy(query, sortBy);
                    break;
                case SortDirection.Desc:
                    query = Queryable.OrderByDescending(query, sortBy);
                    break;
            }
            return query;
        }

        public IQueryable<TEntity> GetPagingQuery(Expression<Func<TEntity, bool>> predicate, PagingModel paging, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = GetListQuery(predicate, cancellationToken);
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

            paging.Total = query.Count();
            if (paging.PageSize.HasValue)
            {
                query = query.Skip(paging.PageIndex * paging.PageSize.Value).Take(paging.PageSize.Value);
                paging.TotalPage = (int)Math.Ceiling((double)paging.Total / paging.PageSize.Value);
            }

            return query;
        }

        public virtual async Task<TEntity> GetEntityByIdAsync(TPrimaryKey id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Table.Where(m => m.Id.Equals(id)).SelectMembers(FilterSelectedFields()).AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        }
        #endregion

        public void UpdateCacheSettings(bool isCache, string cacheFolder = null)
        {
            IsCache = CacheService != null && isCache;
            CacheFolder = cacheFolder ?? typeof(TEntity).FullName;
        }

        public void SetCacheService(MixCacheService cacheService)
        {
            CacheService ??= cacheService;
        }

        public void SetSelectedMembers(string[] selectMembers)
        {
            var properties = typeof(TView).GetProperties().Select(p => p.Name);
            SelectedMembers = selectMembers.Where(m => properties.Contains(m.ToTitleCase())).ToArray();
            if (!SelectedMembers.Any(m => m == "Id"))
            {
                SelectedMembers = SelectedMembers.Prepend("Id").ToArray();
            }
            var arrIndex = properties
            .Select((prop, index) => new { Property = prop, Index = index })
            .Where(x => SelectedMembers.Any(m => m.ToLower() == x.Property.ToLower()))
            .OrderBy(x => x.Index)
            .Select(x => x.Index)
            .ToArray();
            CacheFilename = string.Join('-', arrIndex);
        }

        public virtual async Task<bool> CheckIsExistsAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return entity != null && await Table.AnyAsync(e => e.Id.Equals(entity.Id), cancellationToken);
        }

        public virtual async Task<bool> CheckIsExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Table.AnyAsync(predicate, cancellationToken);
        }

        #region View Async

        public virtual async Task<TView> GetSingleAsync(TPrimaryKey id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsCache && CacheService != null && CacheService.IsCacheEnabled)
            {
                var key = $"{id}/{typeof(TView).FullName}";
                var result = await CacheService.GetAsync<TView>(key, CacheFolder, CacheFilename, cancellationToken);
                if (result != null)
                {
                    if (CacheFilename == "full")
                    {
                        result.SetUowInfo(UowInfo, CacheService);
                        await result.ExpandView(cancellationToken);
                        if (CacheService != null)
                        {
                            result.SetCacheService(CacheService);
                        }
                    }
                    return result;
                }
            }
            var entity = await GetEntityByIdAsync(id, cancellationToken);
            return await GetSingleViewAsync(entity, cancellationToken);
        }

        public virtual async Task<TView> GetSingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entity = await Table.AsNoTracking()
                            .Where(predicate)
                            .SelectMembers(KeyMembers)
                            .SingleOrDefaultAsync(cancellationToken);
            if (entity != null)
            {
                return await GetSingleAsync(entity.Id, cancellationToken);
            }
            return null;
        }

        public virtual async Task<TView> GetFirstAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entity = await Table
                .AsNoTracking()
                .Where(predicate)
                .SelectMembers(FilterSelectedFields())
                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                return await GetSingleAsync(entity.Id, cancellationToken);
            }
            return null;
        }

        public virtual async Task<List<TView>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = GetListQuery(predicate, cancellationToken);
            var result = await ToListViewModelAsync(query, cancellationToken);
            return result;
        }

        public virtual async Task<List<TView>> GetSortedListAsync(
            Expression<Func<TEntity, bool>> predicate,
            string sortby,
            SortDirection direction,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = GetSortedQuery(predicate, sortby, direction, cancellationToken);
            var result = await ToListViewModelAsync(query, cancellationToken);
            return result;
        }

        public virtual async Task<List<TView>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            BeginUow();
            var query = Table.AsNoTracking().Where(predicate);
            var entities = await GetEntitiesAsync(query, cancellationToken);
            return await ParseEntitiesAsync(entities, cancellationToken);
        }

        public virtual async Task<PagingResponseModel<TView>> GetPagingAsync(
            Expression<Func<TEntity, bool>> predicate,
            PagingModel paging,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            BeginUow();
            var query = GetPagingQuery(predicate, paging, cancellationToken);
            return await ToPagingViewModelAsync(query, paging, cancellationToken: cancellationToken);
        }

        #endregion

        #region Helper
        #region Private methods

        private string[] FilterSelectedFields()
        {
            var modelProperties = typeof(TEntity).GetProperties();
            return SelectedMembers.Where(p => modelProperties.Any(m => m.Name == p)).Select(p => p).ToArray();
        }

        protected virtual Task<TView> BuildViewModel(TEntity entity)
        {
            ConstructorInfo classConstructor = typeof(TView).GetConstructor(new Type[] { typeof(TEntity), typeof(UnitOfWorkInfo) });
            return Task.FromResult((TView)classConstructor.Invoke(new object[] { entity, UowInfo }));
        }

        public async Task<List<TView>> ToListViewModelAsync(IQueryable<TEntity> source, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entities = await source.SelectMembers(KeyMembers).AsNoTracking().ToListAsync(cancellationToken);
            var data = await ParseEntitiesAsync(entities, cancellationToken);
            return data;
        }

        protected async Task<PagingResponseModel<TView>> ToPagingViewModelAsync(
            IQueryable<TEntity> source,
            PagingModel pagingData,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entities = await GetEntitiesAsync(source, cancellationToken);
            List<TView> data = await ParseEntitiesAsync(entities, cancellationToken);

            return new PagingResponseModel<TView>(data, pagingData);
        }

        protected async Task<List<TEntity>> GetEntitiesAsync(IQueryable<TEntity> source, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await source.SelectMembers(KeyMembers).ToListAsync(cancellationToken);
        }

        protected async Task<List<TView>> ParseEntitiesAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<TView> data = new List<TView>();

            foreach (var entity in entities)
            {
                var view = await GetSingleAsync(entity.Id, cancellationToken);
                data.Add(view);
            }

            return data;
        }

        protected async Task<TView> GetSingleViewAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entity != null)
            {
                TView result = GetViewModel(entity);

                if (result != null && IsCache && CacheService != null)
                {
                    var key = $"{entity.Id}/{typeof(TView).FullName}";
                    if (CacheFilename == "full")
                    {
                        result.SetUowInfo(UowInfo, CacheService);
                        await result.ExpandView(cancellationToken);
                        await CacheService.SetAsync(key, result, CacheFolder, CacheFilename, cancellationToken);
                    }
                    else
                    {
                        var obj = ReflectionHelper.GetMembers(result, SelectedMembers);
                        await CacheService.SetAsync(key, obj, CacheFolder, CacheFilename, cancellationToken);
                    }
                }

                return result;
            }

            return default;
        }

        protected TView GetViewModel(TEntity entity)
        {
            ConstructorInfo classConstructor = typeof(TView)
                .GetConstructor([typeof(TEntity), typeof(UnitOfWorkInfo)]);

            return (TView)classConstructor.Invoke([entity, UowInfo]);
        }

        #endregion

        protected LambdaExpression GetLambda(string propName, bool isGetDefault = true)
        {
            var parameter = Expression.Parameter(typeof(TEntity));
            var type = typeof(TEntity);
            var prop = Array.Find(type.GetProperties(), p => p.Name == propName.ToTitleCase());
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
