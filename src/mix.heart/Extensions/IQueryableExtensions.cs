using System;
using System.Linq;
using System.Linq.Expressions;

namespace Mix.Heart.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TEntity> SelectMembers<TEntity>(this IQueryable<TEntity> source, params string[] memberNames)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "model");
            var bindings = memberNames
                .Select(name => Expression.Property(parameter, name))
                .Select(member => Expression.Bind(member.Member, member))
                ;
            var body = Expression.MemberInit(Expression.New(typeof(TEntity)), bindings);
            var selector = Expression.Lambda<Func<TEntity, TEntity>>(body, parameter);
            return source.Select(selector);
        }
    }
}