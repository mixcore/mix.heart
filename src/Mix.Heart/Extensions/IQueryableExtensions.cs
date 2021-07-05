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
    }
}