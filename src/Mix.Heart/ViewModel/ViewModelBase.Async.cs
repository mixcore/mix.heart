using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Helpers;
using Mix.Heart.Infrastructure.Interfaces;
using Mix.Heart.Model;
using Mix.Heart.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {
        
        #region Async


        public async Task DeleteAsync(UnitOfWorkInfo uowInfo = null, IMixMediator consumer = null)
        {
            try
            {
                BeginUow(uowInfo, consumer);
                await DeleteHandlerAsync();
                await PublishAsync(this, MixViewModelAction.Delete, true);
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

        protected virtual async Task DeleteHandlerAsync()
        {
            await Repository.DeleteAsync(Id);
        }

        public async Task<TPrimaryKey> SaveAsync(UnitOfWorkInfo uowInfo = null, IMixMediator consumer = null)
        {
            try
            {
                BeginUow(uowInfo, consumer);
                await Validate();
                if (!IsValid)
                {
                    await HandleExceptionAsync(new MixException(MixErrorStatus.Badrequest, Errors.Select(e => e.ErrorMessage).ToArray()));
                }
                var entity = await SaveHandlerAsync();
                await PublishAsync(this, MixViewModelAction.Save, true);
                await CompleteUowAsync();
                return entity.Id;
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
                return default;
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        public async Task<TPrimaryKey> SaveFieldsAsync(IEnumerable<EntityPropertyModel> properties, UnitOfWorkInfo uowInfo = null, IMixMediator consumer = null)
        {
            try
            {
                BeginUow(uowInfo, consumer);
                foreach (var property in properties)
                {
                    // check if field name is exist
                    var lamda = ReflectionHelper.GetLambda<TEntity>(property.PropertyName);
                    if (lamda != null)
                    {
                        ReflectionHelper.SetPropertyValue(this, property);
                    }
                    else
                    {
                        await HandleExceptionAsync(new MixException(MixErrorStatus.Badrequest, $"Invalid Property {property.PropertyName}"));
                    }
                }
                await Validate();
                var entity = await ParseEntity(this);
                await Repository.SaveAsync(entity);
                await CompleteUowAsync();
                return entity.Id;
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex);
                return default;
            }
            finally
            {
                await CloseUowAsync();
            }
        }

        // Override this method if need
        protected virtual async Task<TEntity> SaveHandlerAsync()
        {
            var entity = await ParseEntity(this);
            await Repository.SaveAsync(entity);
            await SaveEntityRelationshipAsync(entity);
            return entity;
        }

        // Override this method if need
        protected virtual Task SaveEntityRelationshipAsync(TEntity parentEntity)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
