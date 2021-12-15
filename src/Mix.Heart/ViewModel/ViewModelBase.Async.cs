using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Helpers;
using Mix.Heart.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    {
        #region Async

        public async Task DeleteAsync()
        {
            try
            {
                BeginUow();
                await DeleteHandlerAsync();
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

        public async Task<TPrimaryKey> SaveAsync()
        {
            try
            {
                BeginUow();
                await Validate();
                if (!IsValid)
                {
                    await HandleExceptionAsync(new MixException(MixErrorStatus.Badrequest, Errors.Select(e => e.ErrorMessage).ToArray()));
                }
                var entity = await SaveHandlerAsync();
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

        public async Task<TPrimaryKey> SaveFieldsAsync(IEnumerable<EntityPropertyModel> properties)
        {
            try
            {
                BeginUow();
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
                var entity = await ParseEntity();
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

        #region virtual methods

        // Override this method
        protected virtual async Task<TEntity> SaveHandlerAsync()
        {
            var entity = await ParseEntity();
            await Repository.SaveAsync(entity);
            await SaveEntityRelationshipAsync(entity);
            return entity;
        }

        // Override this method
        protected virtual Task SaveEntityRelationshipAsync(TEntity parentEntity)
        {
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}
