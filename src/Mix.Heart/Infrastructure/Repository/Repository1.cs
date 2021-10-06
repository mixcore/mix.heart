using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
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
        where TView: ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    {
        public Repository(UnitOfWorkInfo uowInfo) : base(uowInfo) { }
        public Repository(TDbContext dbContext) : base(dbContext) { }

        public Repository()
        {
        }

        #region Async

        public virtual async Task<int> MaxAsync(Expression<Func<TEntity, int>> predicate)
        {
            return await GetAllQuery().MaxAsync(predicate);
        }

        public virtual async Task CreateAsync(TEntity entity, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(uowInfo);
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

        public virtual async Task UpdateAsync(TEntity entity, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(uowInfo);

                if (!await CheckIsExistsAsync(entity))
                {
                    await HandleExceptionAsync(new EntityNotFoundException(entity.Id.ToString()));
                    return;
                }

                Context.Entry(entity).State = EntityState.Modified;
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

        public virtual async Task SaveAsync(TEntity entity, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(uowInfo);

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

        public virtual async Task DeleteAsync(TPrimaryKey id, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(uowInfo);
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    await HandleExceptionAsync(new MixException(MixErrorStatus.NotFound, id));
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
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

        public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(uowInfo);
                var entity = await GetSingleAsync(predicate);
                if (entity == null)
                {
                    await HandleExceptionAsync(new EntityNotFoundException());
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
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

        public virtual async Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(uowInfo);

                await Context.Set<TEntity>().Where(predicate).ForEachAsync(
                    m => Context.Entry(m).State = EntityState.Deleted
                    );

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

        public virtual async Task DeleteAsync(TEntity entity, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow(uowInfo);

                if (!await CheckIsExistsAsync(entity))
                {
                    await HandleExceptionAsync(new EntityNotFoundException(entity.Id.ToString()));
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
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
        #endregion
    }
}
