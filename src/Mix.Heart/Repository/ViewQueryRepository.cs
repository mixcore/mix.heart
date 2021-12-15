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

namespace Mix.Heart.Repository {
  public class ViewQueryRepository<TDbContext, TEntity, TPrimaryKey, TView>
      : RepositoryBase<TDbContext>
      where TDbContext : DbContext
      where TEntity : class, IEntity<TPrimaryKey>
      where TPrimaryKey : IComparable
      where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView> {
    public ViewQueryRepository(TDbContext dbContext) : base(dbContext) {
      CacheService = new();
      SelectedMembers = FilterSelectedFields();
    }

    public ViewQueryRepository(UnitOfWorkInfo unitOfWorkInfo)
        : base(unitOfWorkInfo) {
      CacheService = new();
      SelectedMembers = FilterSelectedFields();
    }

    protected MixCacheService CacheService { get; set; }

    public string CacheFilename { get; private set; } = "full";

    public string[] SelectedMembers { get; private set; }

    protected string[] KeyMembers {
      get { return ReflectionHelper.GetKeyMembers(Context, typeof(TEntity)); }
    }

    protected DbSet<TEntity> Table => Context?.Set<TEntity>();

#region IQueryable

    public IQueryable<TEntity>
    GetListQuery(Expression<Func<TEntity, bool>> predicate) {
      return Table.Where(predicate);
    }

    public IQueryable<TEntity>
    GetPagingQuery(Expression<Func<TEntity, bool>> predicate,
                   IPagingModel paging) {
      var query = GetListQuery(predicate);
      paging.Total = query.Count();
      dynamic sortBy = GetLambda(paging.SortBy);

      switch (paging.SortDirection) {
      case SortDirection.Asc:
        query = Queryable.OrderBy(query, sortBy);
        break;
      case SortDirection.Desc:
        query = Queryable.OrderByDescending(query, sortBy);
        break;
      }

      if (paging.PageSize.HasValue) {
        query = query.Skip(paging.PageIndex * paging.PageSize.Value)
                    .Take(paging.PageSize.Value);
      }

      return query;
    }

    public virtual async Task<TEntity> GetByIdAsync(TPrimaryKey id) {
      return await Table.SelectMembers(SelectedMembers)
          .AsNoTracking()
          .SingleOrDefaultAsync(m => m.Id.Equals(id));
    }
#endregion

    public void SetSelectedMembers(string[] selectMembers) {
      SelectedMembers = selectMembers;
      var properties = typeof(TView).GetProperties().Select(p => p.Name);
      var arrIndex =
          properties
              .Select((prop, index) => new { Property = prop, Index = index })
              .Where(x => selectMembers.Any(m => m.ToLower() ==
                                                 x.Property.ToLower()))
              .Select(x => x.Index.ToString())
              .ToArray();
      CacheFilename = string.Join('-', arrIndex);
      }

      public virtual async Task<bool> CheckIsExistsAsync(TEntity entity) {
        return await Table.AnyAsync(e => e.Id.Equals(entity.Id));
      }

      public virtual async Task<bool> CheckIsExistsAsync(
          Expression<Func<TEntity, bool>> predicate) {
        return await Table.AnyAsync(predicate);
    }

#region View Async

    public virtual async Task<TView> GetSingleAsync(TPrimaryKey id) {
      var entity = await GetByIdAsync(id);
      if (entity != null) {
        return await ParseEntityAsync(entity);
      }
      throw new MixException(MixErrorStatus.NotFound, id);
    }

    public virtual async Task<TView>
    GetSingleAsync(Expression<Func<TEntity, bool>> predicate) {
      var entity = await Table.AsNoTracking().SingleOrDefaultAsync(predicate);
      if (entity != null) {
        return await ParseEntityAsync(entity);
      }
      return null;
    }

    public virtual async Task<TView>
    GetFirstAsync(Expression<Func<TEntity, bool>> predicate) {
      var entity = await Table.AsNoTracking().FirstOrDefaultAsync(predicate);
      if (entity != null) {
        return await ParseEntityAsync(entity);
      }
      return null;
    }

    public virtual async Task<List<TView>>
    GetListAsync(Expression<Func<TEntity, bool>> predicate,
                 MixCacheService cacheService = null) {
      var query = GetListQuery(predicate);
      var result = await ToListViewModelAsync(query);
      return result;
    }

    public virtual async Task<PagingResponseModel<TView>>
    GetPagingAsync(Expression<Func<TEntity, bool>> predicate,
                   IPagingModel paging, MixCacheService cacheService = null) {
      BeginUow();
      var query = GetPagingQuery(predicate, paging);
      return await ToPagingViewModelAsync(query, paging);
    }

#endregion

#region Helper
#region Private methods

    private string[] FilterSelectedFields() {
      var viewProperties = typeof(TView).GetProperties();
      var modelProperties = typeof(TEntity).GetProperties();
      return viewProperties
          .Where(p => modelProperties.Any(m => m.Name == p.Name))
          .Select(p => p.Name)
          .ToArray();
    }

    protected virtual Task<TView> BuildViewModel(TEntity entity) {
      ConstructorInfo classConstructor = typeof(TView).GetConstructor(
          new Type[] { typeof(TEntity), typeof(UnitOfWorkInfo) });
      return Task.FromResult(
          (TView)classConstructor.Invoke(new object[] { entity, UowInfo }));
    }

    public async Task<List<TView>>
    ToListViewModelAsync(IQueryable<TEntity> source,
                         MixCacheService cacheService = null) {
      try {
        var entities = await source.SelectMembers(SelectedMembers)
                           .AsNoTracking()
                           .ToListAsync();

        List<TView> data = await ParseEntitiesAsync(entities);

        return data;
      } catch (Exception ex) {
        throw new MixException(ex.Message);
      }
    }

    protected async Task<PagingResponseModel<TView>>
    ToPagingViewModelAsync(IQueryable<TEntity> source, IPagingModel pagingData,
                           MixCacheService cacheService = null) {
      try {
        var entities = await GetEntities(source);
        List<TView> data = await ParseEntitiesAsync(entities);

        return new PagingResponseModel<TView>(data, pagingData);
      } catch (Exception ex) {
        throw new MixException(ex.Message);
      }
    }

    protected async Task<List<TEntity>>
    GetEntities(IQueryable<TEntity> source) {
      return await source.SelectMembers(SelectedMembers).ToListAsync();
    }

    protected async Task<List<TView>>
    ParseEntitiesAsync(List<TEntity> entities) {
      List<TView> data = new List<TView>();

      foreach (var entity in entities) {
        var view = await ParseEntityAsync(entity);
        data.Add(view);
      }
      return data;
    }

    protected async Task<TView> ParseEntityAsync(TEntity entity) {
      TView result = null;
      if (CacheService != null && CacheService.IsCacheEnabled) {
        result = await CacheService.GetAsync<TView>(
            $"{entity.Id}/{typeof(TView).FullName}", typeof(TEntity),
            CacheFilename);
      }

      if (result == null) {
        result = GetViewModel(entity);

        if (result != null && CacheService != null) {
          if (CacheFilename == "full") {
            await CacheService.SetAsync($"{entity.Id}/{typeof(TView).FullName}",
                                        result, typeof(TEntity), CacheFilename);
          } else {
            var obj = ReflectionHelper.GetMembers(result, SelectedMembers);
            await CacheService.SetAsync($"{entity.Id}/{typeof(TView).FullName}",
                                        obj, typeof(TEntity), CacheFilename);
          }
        }
      }
      result.SetUowInfo(UowInfo);
      await result.ExpandView(CacheService);
      return result;
    }

    protected TView GetViewModel(TEntity entity) {
      ConstructorInfo classConstructor = typeof(TView).GetConstructor(
          new Type[] { typeof(TEntity), typeof(MixCacheService),
                       typeof(UnitOfWorkInfo) });
      return (TView)classConstructor.Invoke(
          new object[] { entity, CacheService, UowInfo });
    }

#endregion

    protected LambdaExpression GetLambda(string propName,
                                         bool isGetDefault = true) {
      var parameter = Expression.Parameter(typeof(TEntity));
      var type = typeof(TEntity);
      var prop = Array.Find(type.GetProperties(), p => p.Name == propName);
      if (prop == null && isGetDefault) {
        propName = "Id";
      } var memberExpression = Expression.Property(parameter, propName);
          return Expression.Lambda(memberExpression, parameter);
    }

#endregion
  }
}
