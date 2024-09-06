using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Repository;
using Mix.Heart.Services;
using Mix.Heart.UnitOfWork;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using Mix.Heart.Helpers;
using Mix.Heart.Model;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mix.Heart.ViewModel {
public abstract class ViewModelQueryBase<TDbContext, TEntity, TPrimaryKey,
                                         TView>
    where TPrimaryKey : IComparable
    where TEntity : class, IEntity<TPrimaryKey>
    where TDbContext : DbContext
    where TView : ViewModelQueryBase<TDbContext, TEntity, TPrimaryKey, TView> {
  protected ValidationContext ValidateContext;

  [JsonIgnore]
  public static bool IsCache { get; set; } = true;

  [JsonIgnore]
  public static string CacheFolder { get; set; } =
      $"{typeof(TEntity).Assembly.GetName().Name}_{typeof(TEntity).Name}";

  [JsonIgnore]
  protected bool IsValid { get; set; }

  [JsonIgnore]
  protected UnitOfWorkInfo UowInfo { get; set; }

  [JsonIgnore]
  protected MixCacheService CacheService { get; set; }

  [JsonIgnore]
  protected List<ValidationResult> Errors { get; set; } = [];

  [JsonIgnore]
  protected Repository<TDbContext, TEntity, TPrimaryKey, TView> Repository {
      get; set; }

  [JsonIgnore]
  public List<ModifiedEntityModel> ModifiedEntities { get; set; } = [];

  protected TDbContext Context { get => (TDbContext)UowInfo?.ActiveDbContext; }

  public ViewModelQueryBase() {
    ValidateContext =
        new ValidationContext(this, serviceProvider: null, items: null);
    Repository ??= GetRepository(UowInfo, CacheService);
  }

  public ViewModelQueryBase(TDbContext context) {
    ValidateContext =
        new ValidationContext(this, serviceProvider: null, items: null);
    UowInfo = new UnitOfWorkInfo(context);
    Repository ??= GetRepository(UowInfo, CacheService);
  }

  public ViewModelQueryBase(TEntity entity, UnitOfWorkInfo uowInfo) {
    ValidateContext =
        new ValidationContext(this, serviceProvider: null, items: null);
    SetUowInfo(uowInfo, CacheService);
    ParseView(entity);
  }

  public ViewModelQueryBase(UnitOfWorkInfo unitOfWorkInfo) {
    ValidateContext =
        new ValidationContext(this, serviceProvider: null, items: null);
    SetUowInfo(unitOfWorkInfo, CacheService);
  }

  public virtual Task
  ExpandView(CancellationToken cancellationToken = default) {
    cancellationToken.ThrowIfCancellationRequested();
    return Task.CompletedTask;
  }

  public virtual void
  ParseView<TSource>(TSource sourceObject,
                     CancellationToken cancellationToken = default)
      where TSource : TEntity {
    cancellationToken.ThrowIfCancellationRequested();
    ReflectionHelper.Map(sourceObject, this as TView);
  }

  public void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo,
                         MixCacheService cacheService) {
    if (unitOfWorkInfo != null) {
      UowInfo = unitOfWorkInfo;
      Repository ??= GetRepository(UowInfo, CacheService);
    }

    SetCacheService(cacheService);
  }

  public void SetCacheService(MixCacheService cacheService) {
    CacheService ??= cacheService;
  }

  public static Repository<TDbContext, TEntity, TPrimaryKey, TView>
  GetRepository(UnitOfWorkInfo uowInfo, MixCacheService cacheService,
                bool isCache = true, string cacheFolder = null) {
    return new Repository<TDbContext, TEntity, TPrimaryKey, TView>(
        uowInfo) { IsCache = isCache, CacheFolder = cacheFolder ?? CacheFolder,
                   CacheService = cacheService };
  }

  public static Repository<TDbContext, TEntity, TPrimaryKey, TView>
  GetRootRepository(TDbContext context, MixCacheService cacheService) {
    return new Repository<TDbContext, TEntity, TPrimaryKey, TView>(
        context) { IsCache = cacheService != null, CacheFolder = CacheFolder };
  }
}
}
