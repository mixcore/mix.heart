using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
public class ViewQueryRepository<TDbContext, TEntity, TPrimaryKey, TView>
    : RepositoryBase<TDbContext>
      where TDbContext : DbContext
      where TEntity : class, IEntity<TPrimaryKey>
      where TPrimaryKey : IComparable
      where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
{
    public ViewQueryRepository(TDbContext dbContext) : base(dbContext) { }
    public ViewQueryRepository()
    {
    }

    public ViewQueryRepository(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
    {
    }

    protected string[] SelectedMembers {
        get {
            return FilterSelectedFields();
        }
    }
    protected string[] KeyMembers {
        get {
            return ReflectionHelper.GetKeyMembers(Context, typeof(TEntity));
        }
    }

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
        return await Table.SelectMembers(SelectedMembers).SingleAsync(m => m.Id.Equals(id));
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

    public virtual async Task<TView> GetSingleAsync(TPrimaryKey id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            return await BuildViewModel(entity);
        }
        throw new MixException(MixErrorStatus.NotFound, id);
    }

    public virtual async Task<TView> GetSingleAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var entity = await Table.SingleOrDefaultAsync(predicate);
        if (entity != null)
        {
            var result = await BuildViewModel(entity);
            return result;
        }
        return null;
    }

    public virtual async Task<List<TView>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate, UnitOfWorkInfo uowInfo = null)
    {
        BeginUow(uowInfo);
        var query = GetListQuery(predicate);
        var result = await ToListViewModelAsync(query);
        await CloseUowAsync();
        return result;
    }

    public virtual async Task<PagingResponseModel<TView>> GetPagingAsync(
        Expression<Func<TEntity, bool>> predicate, IPagingModel paging, UnitOfWorkInfo uowInfo = null)
    {
        BeginUow(uowInfo);
        var query = GetPagingQuery(predicate, paging);
        return await ToPagingViewModelAsync(query, paging);
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
        bool isCache = false)
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
                var view = await BuildViewModel(entity, UowInfo);
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

    protected async Task<PagingResponseModel<TView>> ToPagingViewModelAsync(
        IQueryable<TEntity> source,
        IPagingModel pagingData,
        bool isCache = false)
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

    private List<TView> GetListCachedData(
        List<TEntity> entities,
        ViewQueryRepository<TDbContext, TEntity, TPrimaryKey, TView> repository,
        params string[] keys)
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
        TEntity entity,
        ViewQueryRepository<TDbContext, TEntity, TPrimaryKey, TView> repository,
        string[] keys)
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
