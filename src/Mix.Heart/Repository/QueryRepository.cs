using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Exceptions;
using Mix.Heart.Helpers;
using Mix.Heart.Models;
using Mix.Heart.Services;
using Mix.Heart.UnitOfWork;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public class QueryRepository<TDbContext, TEntity, TPrimaryKey>
        : RepositoryBase<TDbContext>
        where TDbContext : DbContext
        where TEntity : class, IEntity<TPrimaryKey>
        where TPrimaryKey : IComparable
    {

        #region Properties

        protected MixCacheService CacheService { get; set; }

        public string CacheFilename { get; protected set; } = "full";

        public string[] SelectedMembers { get; protected set; }

        protected string[] KeyMembers { get { return ReflectionHelper.GetKeyMembers(Context, typeof(TEntity)); } }

        protected DbSet<TEntity> Table => Context?.Set<TEntity>();

        #endregion

        public QueryRepository(TDbContext dbContext) : base(dbContext) { }

        public QueryRepository()
        {
            SelectedMembers = FilterSelectedFields();
        }

        public QueryRepository(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
        {
            SelectedMembers = FilterSelectedFields();
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
                       PagingModel paging)
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

        public async Task<PagingResponseModel<TEntity>> GetPagingEntitiesAsync(
            Expression<Func<TEntity, bool>> predicate,
            PagingModel paging,
            CancellationToken cancellationToken = default)
        {
            var source = GetPagingQuery(predicate, paging);
            return await ToPagingEntityAsync(source, paging, cancellationToken);
        }

        public async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await GetAllQuery().SingleOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<TEntity> GetByIdAsync(TPrimaryKey id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }

        public virtual Task<int> MaxAsync(Expression<Func<TEntity, int>> predicate, CancellationToken cancellationToken = default)
        {
            return GetAllQuery().MaxAsync(predicate, cancellationToken);
        }

        public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return GetAllQuery().FirstOrDefaultAsync(predicate, cancellationToken);
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

        #region Helper
        private string[] FilterSelectedFields()
        {
            var properties = typeof(TEntity).GetProperties();
            return properties.Where(p => p.PropertyType.IsSerializable).Select(p => p.Name).ToArray();
        }

        protected async Task<PagingResponseModel<TEntity>> ToPagingEntityAsync(
           IQueryable<TEntity> source,
           PagingModel pagingData,
           CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = await source.ToListAsync(cancellationToken);

                return new PagingResponseModel<TEntity>(entities, pagingData);
            }
            catch (Exception ex)
            {
                throw new MixException(ex.Message);
            }
        }
        protected LambdaExpression GetLambda(string propName, bool isGetDefault = true)
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
