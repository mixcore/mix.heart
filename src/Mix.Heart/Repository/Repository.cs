using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Infrastructure.Exceptions;
using Mix.Heart.UnitOfWork;
using Mix.Heart.ViewModel;
using System;
using System.Linq;
using System.Linq.Expressions;
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

        public virtual async Task<int> MaxAsync(Expression<Func<TEntity, int>> predicate)
        {
            return await Table.MaxAsync(predicate);
        }

        public virtual async Task CreateAsync(TEntity entity)
        {
            try
            {
                BeginUow();
                Context.Entry(entity).State = EntityState.Added;
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
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

        public virtual async Task UpdateAsync(TEntity entity)
        {
            try
            {
                BeginUow();

                if (!await CheckIsExistsAsync(entity))
                {
                    await HandleExceptionAsync(new EntityNotFoundException(entity.Id.ToString()));
                    return;
                }

                Context.Entry(entity).State = EntityState.Modified;
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
                await CacheService.RemoveCacheAsync(entity.Id, typeof(TEntity));
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

        public virtual async Task SaveAsync(TEntity entity)
        {
            try
            {
                BeginUow();

                if (await CheckIsExistsAsync(entity))
                {
                    await UpdateAsync(entity);
                }
                else { await CreateAsync(entity); }
                await CompleteUowAsync();
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

        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            try
            {
                var entity = await GetEntityByIdAsync(id);
                await DeleteAsync(entity);
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

        public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
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
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
                await CacheService.RemoveCacheAsync(entity.Id, typeof(TEntity));
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

        public virtual async Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                var entities = Context.Set<TEntity>().Where(predicate);
                foreach (var entity in entities)
                {
                    await DeleteAsync(entity);
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

        public virtual async Task DeleteAsync(TEntity entity)
        {
            try
            {
                BeginUow();

                if (!await CheckIsExistsAsync(entity))
                {
                    await HandleExceptionAsync(new EntityNotFoundException(entity.Id.ToString()));
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
                await Context.SaveChangesAsync();
                await CompleteUowAsync();
                await CacheService.RemoveCacheAsync(entity.Id, typeof(TEntity));
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
