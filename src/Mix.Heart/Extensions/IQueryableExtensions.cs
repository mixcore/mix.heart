using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Exceptions;
using Mix.Heart.Helpers;
using Mix.Heart.Model;
using Mix.Heart.Repository;
using Mix.Heart.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Mix.Heart.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TEntity> SelectMembers<TEntity>(this IQueryable<TEntity> source, params string[] memberNames)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "model");
            var bindings = memberNames
                .Select(name => Expression.PropertyOrField(parameter, name))
                .Select(member => Expression.Bind(member.Member, member));
            var body = Expression.MemberInit(Expression.New(typeof(TEntity)), bindings);
            var selector = Expression.Lambda<Func<TEntity, TEntity>>(body, parameter);
            return source.Select(selector);
        }

        public static async Task<PagingResponseModel<TView>> ToPagingViewModelAsync<TDbContext, TView, TEntity, TPrimaryKey>(
            this IQueryable<TEntity> source, 
            QueryRepository<TDbContext, TEntity, TPrimaryKey> repository,
            IPagingModel pagingData,
            bool isCache)
                where TPrimaryKey: IComparable
                where TDbContext: DbContext
                where TEntity : class, IEntity<TPrimaryKey>
                where TView : IViewModel<TPrimaryKey>
        {
            try
            {
                var keys = ReflectionHelper.GetKeyMembers(repository.Context, typeof(TEntity));
                var members = isCache ? keys
                                        : ReflectionHelper.FilterSelectedFields<TView, TEntity>();
                var entities = await source.SelectMembers(members).ToListAsync();

                List<TView> data = new List<TView>();
                ConstructorInfo classConstructor = typeof(TView).GetConstructor(new Type[] { typeof(TEntity)});
                foreach (var entity in entities)
                {
                    var view = (TView)classConstructor.Invoke(new object[] { entity });
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
                throw new MixHttpResponseException(ex.Message);
            }
        }

        private static List<TView> GetListCachedData<TView, TEntity, TPrimaryKey, TDbContext>(
            List<TEntity> entities, 
            QueryRepository<TDbContext, TEntity, TPrimaryKey> repository,
            params string[] keys)
                where TView : IViewModel<TPrimaryKey>
                where TEntity : class, IEntity<TPrimaryKey>
                where TPrimaryKey : IComparable
                where TDbContext : DbContext
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

        private static IViewModel<TPrimaryKey> GetCachedData<TEntity, TPrimaryKey, TDbContext>(TEntity entity, QueryRepository<TDbContext, TEntity, TPrimaryKey> repository, string[] keys)
            where TEntity : class, IEntity<TPrimaryKey>
            where TPrimaryKey : IComparable
            where TDbContext : DbContext
        {
            throw new NotImplementedException();
        }
    }
}