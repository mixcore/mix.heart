﻿using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Infrastructure.Exceptions;
using Mix.Heart.UnitOfWork;
using Mix.Heart.ViewModel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
public class Repository<TDbContext, TEntity, TPrimaryKey, TView>
    : ViewQueryRepository<TDbContext, TEntity, TPrimaryKey, TView>
      where TPrimaryKey : IComparable
      where TDbContext : DbContext
      where TEntity : class, IEntity<TPrimaryKey>
      where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
{
    public Repository(UnitOfWorkInfo uowInfo) : base(uowInfo) { }
    public Repository(TDbContext dbContext) : base(dbContext) { }

    #region Async

    public virtual async Task<int> MaxAsync(Expression<Func<TEntity, int>> predicate, CancellationToken cancellationToken = default)
    {
        return await Table.MaxAsync(predicate, cancellationToken);
    }

    public virtual async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            BeginUow();
            Context.Entry(entity).State = EntityState.Added;
            await Context.SaveChangesAsync(cancellationToken);
            await CompleteUowAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex);
        }
        finally
        {
            await CloseUowAsync();
        }
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            BeginUow();

            if (!await CheckIsExistsAsync(entity, cancellationToken))
            {
                await HandleExceptionAsync(new EntityNotFoundException(entity.Id.ToString()));
                return;
            }

            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync(cancellationToken);
            await CompleteUowAsync(cancellationToken);
            await CacheService.RemoveCacheAsync(entity.Id, typeof(TEntity), cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex);
        }
        finally
        {
            await CloseUowAsync();
        }
    }

    public virtual async Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            BeginUow();

            if (await CheckIsExistsAsync(entity, cancellationToken))
            {
                await UpdateAsync(entity, cancellationToken);
            }
            else
            {
                await CreateAsync(entity, cancellationToken);
            }
            await CompleteUowAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex);
        }
        finally
        {
            await CloseUowAsync();
        }
    }

    public virtual async Task DeleteAsync(TPrimaryKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await GetEntityByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await DeleteAsync(entity, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex);
        }
        finally
        {
            await CloseUowAsync();
        }
    }

    public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            BeginUow();
            var entity = Context.Set<TEntity>().Single(predicate);
            if (entity == null)
            {
                await HandleExceptionAsync(new EntityNotFoundException());
                return;
            }

            Context.Set<TEntity>().Remove(entity).State = EntityState.Deleted;
            await Context.SaveChangesAsync(cancellationToken);
            await CompleteUowAsync(cancellationToken);
            await CacheService.RemoveCacheAsync(entity.Id, typeof(TEntity), cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex);
        }
        finally
        {
            await CloseUowAsync();
        }
    }

    public virtual async Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = Context.Set<TEntity>().Where(predicate);
            foreach (var entity in entities)
            {
                await DeleteAsync(entity, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex);
        }
        finally
        {
            await CloseUowAsync();
        }
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            BeginUow();

            if (!await CheckIsExistsAsync(entity, cancellationToken))
            {
                await HandleExceptionAsync(new EntityNotFoundException(entity.Id.ToString()));
                return;
            }

            Context.Entry(entity).State = EntityState.Deleted;
            await Context.SaveChangesAsync(cancellationToken);
            await CompleteUowAsync(cancellationToken);
            await CacheService.RemoveCacheAsync(entity.Id, typeof(TEntity), cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex);
        }
        finally
        {
            await CloseUowAsync();
        }
    }
    #endregion
}
}
