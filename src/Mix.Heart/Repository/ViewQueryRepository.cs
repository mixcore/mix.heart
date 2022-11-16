﻿using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
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

namespace Mix.Heart.Repository {
  public class ViewQueryRepository<TDbContext, TEntity, TPrimaryKey, TView>
      : RepositoryBase<TDbContext>
      where TDbContext : DbContext
      where TEntity : class, IEntity<TPrimaryKey>
      where TPrimaryKey : IComparable
      where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView> {
    public ViewQueryRepository(TDbContext dbContext) : base(dbContext) {
      CacheService = new();
      SelectedMembers =
          typeof(TView).GetProperties().Select(m => m.Name).ToArray();
    }

    public ViewQueryRepository(UnitOfWorkInfo unitOfWorkInfo)
        : base(unitOfWorkInfo) {
      CacheService = new();
      SelectedMembers =
          typeof(TView).GetProperties().Select(m => m.Name).ToArray();
    }

    public MixCacheService CacheService { get; set; }

    public string CacheFilename { get; private set; } = "full";

    public string[] SelectedMembers { get; private set; }

    protected string[] KeyMembers {
      get { return ReflectionHelper.GetKeyMembers(Context, typeof(TEntity)); }
    }

    public DbSet<TEntity> Table => Context?.Set<TEntity>();

#region IQueryable

    public IQueryable<TEntity>
    GetListQuery(Expression<Func<TEntity, bool>> predicate) {
      return Table.AsNoTracking().Where(predicate);
    }

    public IQueryable<TEntity>
    GetPagingQuery(Expression<Func<TEntity, bool>> predicate,
                   PagingModel paging) {
      var query = GetListQuery(predicate);
      dynamic sortBy = GetLambda(paging.SortBy);

      switch (paging.SortDirection) {
      case SortDirection.Asc:
        query = Queryable.OrderBy(query, sortBy);
        break;
      case SortDirection.Desc:
        query = Queryable.OrderByDescending(query, sortBy);
        break;
      }

      paging.Total = query.Count();
      if (paging.PageSize.HasValue) {
        query = query.Skip(paging.PageIndex * paging.PageSize.Value)
                    .Take(paging.PageSize.Value);
        paging.TotalPage =
            (int)Math.Ceiling((double)paging.Total / paging.PageSize.Value);
      }

      return query;
    }

    public virtual async Task<TEntity>
    GetEntityByIdAsync(TPrimaryKey id,
                       CancellationToken cancellationToken = default) {
      return await Table.Where(m => m.Id.Equals(id))
          .SelectMembers(FilterSelectedFields())
          .AsNoTracking()
          .SingleOrDefaultAsync(cancellationToken);
    }
#endregion

    public void SetSelectedMembers(string[] selectMembers) {
      var properties = typeof(TView).GetProperties().Select(p => p.Name);
      SelectedMembers =
          SelectedMembers.Where(m => selectMembers.Contains(m.ToCamelCase()))
              .ToArray();
      if (!SelectedMembers.Any(m => m == "Id")) {
        SelectedMembers = SelectedMembers.Prepend("Id").ToArray();
      }
      var arrIndex =
          properties
              .Select((prop, index) => new { Property = prop, Index = index })
              .Where(x => SelectedMembers.Any(m => m.ToLower() ==
                                                   x.Property.ToLower()))
              .OrderBy(x => x.Index)
              .Select(x => x.Index)
              .ToArray();
      CacheFilename = string.Join('-', arrIndex);
      }

      public virtual async Task<bool> CheckIsExistsAsync(
          TEntity entity, CancellationToken cancellationToken = default) {
        return entity != null &&
               await Table.AnyAsync(e => e.Id.Equals(entity.Id),
                                    cancellationToken);
      }

      public virtual async Task<bool> CheckIsExistsAsync(
          Expression<Func<TEntity, bool>> predicate,
          CancellationToken cancellationToken = default) {
        return await Table.AnyAsync(predicate, cancellationToken);
      }

#region View Async

      public virtual async Task<TView> GetSingleAsync(
          TPrimaryKey id, CancellationToken cancellationToken = default) {
        if (CacheService != null && CacheService.IsCacheEnabled) {
          var key = $"{id}/{typeof(TView).FullName}";
          var result = await CacheService.GetAsync<TView>(
              key, typeof(TEntity), CacheFilename, cancellationToken);
          if (result != null) {
            if (CacheFilename == "full") {
              result.SetUowInfo(UowInfo);
              await result.ExpandView(cancellationToken);
            }
            return result;
          }
        }
        var entity = await GetEntityByIdAsync(id, cancellationToken);
        return await GetSingleViewAsync(entity, cancellationToken);
      }

      public virtual async Task<TView> GetSingleAsync(
          Expression<Func<TEntity, bool>> predicate,
          CancellationToken cancellationToken = default) {
        var entity = await Table.AsNoTracking()
                         .Where(predicate)
                         .SelectMembers(KeyMembers)
                         .SingleOrDefaultAsync(cancellationToken);
        if (entity != null) {
          return await GetSingleAsync(entity.Id, cancellationToken);
        }
        return null;
      }

      public virtual async Task<TView> GetFirstAsync(
          Expression<Func<TEntity, bool>> predicate,
          CancellationToken cancellationToken = default) {
        var entity = await Table.AsNoTracking()
                         .Where(predicate)
                         .SelectMembers(FilterSelectedFields())
                         .FirstOrDefaultAsync(cancellationToken);

        if (entity != null) {
          return await GetSingleAsync(entity.Id, cancellationToken);
        }
        return null;
      }

      public virtual async Task<List<TView>> GetListAsync(
          Expression<Func<TEntity, bool>> predicate,
          CancellationToken cancellationToken = default) {
        var query = GetListQuery(predicate);
        var result = await ToListViewModelAsync(query, cancellationToken);
        return result;
      }

      public virtual async Task<List<TView>> GetAllAsync(
          Expression<Func<TEntity, bool>> predicate,
          CancellationToken cancellationToken = default) {
        BeginUow();
        var query = Table.AsNoTracking().Where(predicate);
        var entities = await GetEntitiesAsync(query, cancellationToken);
        return await ParseEntitiesAsync(entities, cancellationToken);
      }

      public virtual async Task<PagingResponseModel<TView>> GetPagingAsync(
          Expression<Func<TEntity, bool>> predicate, PagingModel paging) {
        BeginUow();
        var query = GetPagingQuery(predicate, paging);
        return await ToPagingViewModelAsync(query, paging);
      }

#endregion

#region Helper
#region Private methods

      private string[] FilterSelectedFields() {
        var modelProperties = typeof(TEntity).GetProperties();
        return SelectedMembers.Where(p => modelProperties.Any(m => m.Name == p))
            .Select(p => p)
            .ToArray();
      }

      protected virtual Task<TView> BuildViewModel(TEntity entity) {
        ConstructorInfo classConstructor = typeof(TView).GetConstructor(
            new Type[] { typeof(TEntity), typeof(UnitOfWorkInfo) });
        return Task.FromResult(
            (TView)classConstructor.Invoke(new object[] { entity, UowInfo }));
      }

      public async Task<List<TView>> ToListViewModelAsync(
          IQueryable<TEntity> source,
          CancellationToken cancellationToken = default) {
        try {
          var entities = await source.SelectMembers(KeyMembers)
                             .AsNoTracking()
                             .ToListAsync(cancellationToken);
          var data = await ParseEntitiesAsync(entities, cancellationToken);
          return data;
        } catch (Exception ex) {
          throw new MixException(ex.Message);
        }
      }

      protected async Task<PagingResponseModel<TView>> ToPagingViewModelAsync(
          IQueryable<TEntity> source, PagingModel pagingData,
          MixCacheService cacheService = null,
          CancellationToken cancellationToken = default) {
        try {
          var entities = await GetEntitiesAsync(source, cancellationToken);
          List<TView> data =
              await ParseEntitiesAsync(entities, cancellationToken);

          return new PagingResponseModel<TView>(data, pagingData);
        } catch (Exception ex) {
          throw new MixException(ex.Message);
        }
      }

      protected async Task<List<TEntity>> GetEntitiesAsync(
          IQueryable<TEntity> source,
          CancellationToken cancellationToken = default) {
        return await source.SelectMembers(KeyMembers)
            .ToListAsync(cancellationToken);
      }

      protected async Task<List<TView>> ParseEntitiesAsync(
          List<TEntity> entities,
          CancellationToken cancellationToken = default) {
        List<TView> data = new List<TView>();

        foreach (var entity in entities) {
          var view = await GetSingleAsync(entity.Id, cancellationToken);
          data.Add(view);
        }
        return data;
      }

      protected async Task<TView> GetSingleViewAsync(
          TEntity entity, CancellationToken cancellationToken = default) {
        if (entity != null) {
          TView result = GetViewModel(entity);

          if (result != null && CacheService != null) {
            var key = $"{entity.Id}/{typeof(TView).FullName}";
            if (CacheFilename == "full") {
              result.SetUowInfo(UowInfo);
              await result.ExpandView(cancellationToken);
              await CacheService.SetAsync(key, result, typeof(TEntity),
                                          CacheFilename, cancellationToken);
            } else {
              var obj = ReflectionHelper.GetMembers(result, SelectedMembers);
              await CacheService.SetAsync(key, obj, typeof(TEntity),
                                          CacheFilename, cancellationToken);
            }
          }

          return result;
        }
        return default;
      }

      protected TView GetViewModel(TEntity entity) {
        ConstructorInfo classConstructor = typeof(TView).GetConstructor(
            new Type[] { typeof(TEntity), typeof(UnitOfWorkInfo) });
        return (TView)classConstructor.Invoke(new object[] { entity, UowInfo });
      }

#endregion

      protected LambdaExpression GetLambda(string propName,
                                           bool isGetDefault = true) {
        var parameter = Expression.Parameter(typeof(TEntity));
        var type = typeof(TEntity);
        var prop = Array.Find(type.GetProperties(),
                              p => p.Name == propName.ToTitleCase());
        if (prop == null && isGetDefault) {
          propName = "Id";
        } var memberExpression = Expression.Property(parameter, propName);
            return Expression.Lambda(memberExpression, parameter);
      }

#endregion
    }
  }
