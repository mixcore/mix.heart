using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Infrastructure.Exceptions;
using Mix.Heart.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public class CommandRepository<TDbContext, TEntity, TPrimaryKey>
        : QueryRepository<TDbContext, TEntity, TPrimaryKey>
        where TPrimaryKey : IComparable
        where TDbContext : DbContext
        where TEntity : class, IEntity<TPrimaryKey>
    {
        public CommandRepository(UnitOfWorkInfo uowInfo) : base(uowInfo) { }
        public CommandRepository(TDbContext dbContext) : base(dbContext) { }

        public virtual bool CheckIsExists(TEntity entity)
        {
            return GetAllQuery().Any(e => e.Id.Equals(entity.Id));
        }

        public virtual bool CheckIsExists(Func<TEntity, bool> predicate)
        {
            return GetAllQuery().Any(predicate);
        }

        #region Async

        public virtual async Task CreateAsync(TEntity entity)
        {
            try
            {
                BeginUow();
                Context.Entry(entity).State = EntityState.Added;
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                CloseUow();
            }
            finally
            {
                CompleteUow();
            }
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            try
            {
                BeginUow();

                if (!CheckIsExists(entity))
                {
                    HandleException(new EntityNotFoundException(entity.Id.ToString()));
                    CloseUow();
                    return;
                }

                Context.Entry(entity).State = EntityState.Modified;
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                CloseUow();
            }
            finally
            {
                CompleteUow();
            }
        }

        public virtual async Task SaveAsync(TEntity entity, UnitOfWorkInfo uowInfo = null)
        {
            try
            {
                BeginUow();

                if (CheckIsExists(entity))
                {
                    await UpdateAsync(entity);
                }
                else { await CreateAsync(entity); }

            }
            catch (Exception ex)
            {
                HandleException(ex);
                CloseUow();
            }
            finally
            {
                CompleteUow();
            }
        }

        public virtual async Task DeleteAsync(TPrimaryKey id)
        {
            try
            {
                BeginUow();
                var entity = GetById(id);
                if (entity == null)
                {
                    HandleException(new EntityNotFoundException());
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                CompleteUow();
            }
        }
        
        public virtual async Task DeleteAsync(TEntity entity)
        {
            try
            {
                BeginUow();

                if (!CheckIsExists(entity))
                {
                    HandleException(new EntityNotFoundException(entity.Id.ToString()));
                    CloseUow();
                    return;
                }

                Context.Entry(entity).State = EntityState.Deleted;
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                CloseUow();
            }
            finally
            {
                CompleteUow();
            }
        }
        #endregion
    }
}
