﻿using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Helpers;
using Mix.Heart.Model;
using Mix.Heart.Repository;
using Mix.Heart.Services;
using Mix.Heart.UnitOfWork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel {
public abstract
    partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    : IViewModel
    where TPrimaryKey : IComparable
    where TEntity : class, IEntity<TPrimaryKey>
    where TDbContext : DbContext
    where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView> {
#region Properties

  public TPrimaryKey Id { get; set; }

  public DateTime CreatedDateTime { get; set; }

  public DateTime? LastModified { get; set; }

  public string CreatedBy { get; set; }

  public string ModifiedBy { get; set; }

  public int Priority { get; set; }

  public MixContentStatus Status { get; set; } = MixContentStatus.Published;

  protected ValidationContext ValidateContext;

  public bool IsDeleted { get; set; }

  [Newtonsoft.Json.JsonIgnore]
  public static bool IsCache { get; set; } = true;

  [Newtonsoft.Json.JsonIgnore]

  public static string CacheFolder { get; set; } =
      $"{typeof(TEntity).Assembly.GetName().Name}_{typeof(TEntity).Name}";

  [Newtonsoft.Json.JsonIgnore]
  protected bool IsValid { get; set; }

  [Newtonsoft.Json.JsonIgnore]
  protected UnitOfWorkInfo UowInfo { get; set; }

  [Newtonsoft.Json.JsonIgnore]
  protected MixCacheService CacheService { get; set; }

  [Newtonsoft.Json.JsonIgnore]
  protected List<ValidationResult> Errors { get; set; } =
      new List<ValidationResult>();

  [Newtonsoft.Json.JsonIgnore]
  protected Repository<TDbContext, TEntity, TPrimaryKey, TView> Repository {
      get; set; }

  protected TDbContext Context { get => (TDbContext)UowInfo?.ActiveDbContext; }

  [Newtonsoft.Json.JsonIgnore]
  public List<ModifiedEntityModel> ModifiedEntities { get; set; } = new();

#endregion

#region Constructors

  public ViewModelBase() {
    ValidateContext =
        new ValidationContext(this, serviceProvider: null, items: null);
    Repository ??= GetRepository(UowInfo, CacheService);
  }

  public ViewModelBase(TDbContext context) {
    ValidateContext =
        new ValidationContext(this, serviceProvider: null, items: null);
    UowInfo = new UnitOfWorkInfo(context);
    Repository ??= GetRepository(UowInfo, CacheService);

    _isRoot = true;
  }

  public ViewModelBase(TEntity entity, UnitOfWorkInfo uowInfo) {
    ValidateContext =
        new ValidationContext(this, serviceProvider: null, items: null);
    SetUowInfo(uowInfo, CacheService);
    ParseView(entity);
  }

  public ViewModelBase(UnitOfWorkInfo unitOfWorkInfo) {
    ValidateContext =
        new ValidationContext(this, serviceProvider: null, items: null);
    SetUowInfo(unitOfWorkInfo, CacheService);
  }

#endregion

#region Abstracts
  public virtual void InitDefaultValues(string language = null,
                                        int? cultureId = null) {
    CreatedDateTime = DateTime.UtcNow;
    Status = MixContentStatus.Published;
    IsDeleted = false;
  }

#endregion

  public virtual Task
  ExpandView(CancellationToken cancellationToken = default) {
    cancellationToken.ThrowIfCancellationRequested();
    return Task.CompletedTask;
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

  public virtual async Task Validate(CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (!IsValid) {
      await HandleExceptionAsync(
          new MixException(MixErrorStatus.Badrequest,
                           Errors.Select(e => e.ErrorMessage).ToArray()));
    }
  }

  public void SetDbContext(TDbContext context) {
    UowInfo = new UnitOfWorkInfo(context);
  }

  public void SetCacheService(MixCacheService cacheService) {
    CacheService ??= cacheService;
  }

  public virtual TEntity InitModel() {
    Type classType = typeof(TEntity);
    return (TEntity)Activator.CreateInstance(classType);
  }

  public virtual Task<TEntity>
  ParseEntity(CancellationToken cancellationToken = default) {
    cancellationToken.ThrowIfCancellationRequested();

    if (IsDefaultId(Id)) {
      InitDefaultValues();
    }

    var entity = Activator.CreateInstance<TEntity>();
    ReflectionHelper.Map(this as TView, entity);
    return Task.FromResult(entity);
  }

  public virtual void
  ParseView<TSource>(TSource sourceObject,
                     CancellationToken cancellationToken = default)
      where TSource : TEntity {
    cancellationToken.ThrowIfCancellationRequested();
    ReflectionHelper.Map(sourceObject, this as TView);
  }

  public bool IsDefaultId(TPrimaryKey id) {
    return (id.GetType() == typeof(Guid) &&
            Guid.Parse(id.ToString()) == Guid.Empty) ||
           (id.GetType() == typeof(int) && int.Parse(id.ToString()) == default);
  }

  public virtual Task
  DuplicateAsync(CancellationToken cancellationToken = default) {
    cancellationToken.ThrowIfCancellationRequested();
    return Task.CompletedTask;
  }

  public virtual void Duplicate() {}

  protected async Task HandleErrorsAsync() {
    await HandleExceptionAsync(
        new MixException(MixErrorStatus.Badrequest,
                         Errors.Select(e => e.ErrorMessage).ToArray()));
  }

  protected virtual async Task HandleExceptionAsync(Exception ex) {
    await Repository.HandleExceptionAsync(ex);
  }

  protected virtual void HandleException(Exception ex) {
    Repository.HandleException(ex);
  }
}
}
