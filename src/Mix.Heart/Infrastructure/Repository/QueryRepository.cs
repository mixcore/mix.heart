using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Model;
using Mix.Heart.UnitOfWork;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public class QueryRepository<TDbContext, TEntity, TPrimaryKey>
        : RepositoryBase<TDbContext>
        where TDbContext : DbContext
        where TEntity : class, IEntity<TPrimaryKey>
        where TPrimaryKey : IComparable
    {
        public QueryRepository(UnitOfWorkInfo uowInfo) : base(uowInfo) { }
        public QueryRepository(TDbContext dbContext) : base(dbContext) { }

        public virtual object GetById(object id)
        {
            return Context.Set<TEntity>().Find(id);
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

        public async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await GetAllQuery().SingleOrDefaultAsync(predicate);
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
            query =
                query.Skip(paging.PageIndex * paging.PageSize).Take(paging.PageIndex);
            return query;
        }

        #endregion

        #region Async

        public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAllQuery().FirstOrDefaultAsync(predicate);
        }

        #endregion

        #region Helper

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
