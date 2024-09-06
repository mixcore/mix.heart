using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Exceptions;
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
        where TView : ViewModelQueryBase<TDbContext, TEntity, TPrimaryKey, TView>
    {
        public Repository(UnitOfWorkInfo uowInfo) : base(uowInfo)
        {
            CacheFolder = $"{typeof(TEntity).Assembly.GetName().Name}_{typeof(TEntity).Name}";
        }
        public Repository(TDbContext dbContext) : base(dbContext)
        {
            CacheFolder = $"{typeof(TEntity).Assembly.GetName().Name}_{typeof(TEntity).Name}";
        }

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
                await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
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
                    throw new MixException(Enums.MixErrorStatus.NotFound, entity.Id.ToString());

                Context.Set<TEntity>().Update(entity);
                await Context.SaveChangesAsync(cancellationToken);
                await CompleteUowAsync(cancellationToken);
                if (CacheService != null)
                {
                    await CacheService.RemoveCacheAsync(entity.Id, CacheFolder, cancellationToken);
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
                var entity = await Context.Set<TEntity>().SingleOrDefaultAsync(predicate, cancellationToken);
                if (entity == null)
                {
                    await HandleExceptionAsync(new EntityNotFoundException());
                    return;
                }

                Context.Set<TEntity>().Remove(entity).State = EntityState.Deleted;
                await Context.SaveChangesAsync(cancellationToken);
                await CompleteUowAsync(cancellationToken);
                await CacheService.RemoveCacheAsync(entity.Id, CacheFolder, cancellationToken);
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
                var entities = await Context.Set<TEntity>().Where(predicate)
                                .ToListAsync();
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
                if (CacheService != null)
                {
                    await CacheService?.RemoveCacheAsync($"{typeof(TEntity).FullName}_{entity.Id}*", CacheFolder, cancellationToken);
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
        #endregion
    }
}
