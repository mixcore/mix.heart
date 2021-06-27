using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel {
  public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
      where TPrimaryKey : IComparable
      where TEntity : class, IEntity<TPrimaryKey>
      where TDbContext : DbContext {
    protected CommandRepository<TDbContext, TEntity, TPrimaryKey> _repository {
      get; set;
    }

    public ViewModelBase() {}

    public ViewModelBase(TEntity entity) { ParseView(entity); }

    public ViewModelBase(UnitOfWorkInfo unitOfWorkInfo) {
      _unitOfWorkInfo = unitOfWorkInfo;
      _repository.SetUowInfo(_unitOfWorkInfo);
    }

    public void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo) {
      _repository.SetUowInfo(unitOfWorkInfo);
    }

#region Async

    public async Task<TPrimaryKey> SaveAsync(UnitOfWorkInfo uowInfo = null) {
      try {
        BeginUow(uowInfo);
        _repository.SetUowInfo(_unitOfWorkInfo);
        var entity = await SaveHandlerAsync();
        return entity.Id;
      } catch (Exception ex) {
        HandleException(ex);
        CloseUow();
        return default;
      } finally {
        CompleteUowAsync();
      }
    }

    // Override this method if need
    protected virtual async Task<TEntity> SaveHandlerAsync() {
      var entity = await ParseEntity(this);
      await _repository.SaveAsync(entity);
      await SaveEntityRelationshipAsync(entity);
      return entity;
    }

    // Override this method if need
    protected virtual Task SaveEntityRelationshipAsync(TEntity parentEntity) {
      return Task.CompletedTask;
    }

#endregion
  }
}
