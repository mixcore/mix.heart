using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Helpers;
using Mix.Heart.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    {
        #region Async

        public async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                BeginUow();
                await DeleteHandlerAsync(cancellationToken);
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

        protected virtual async Task DeleteHandlerAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Repository.SetUowInfo(UowInfo);
            await Repository.DeleteAsync(Id, cancellationToken);
            ModifiedEntities.Add(new(typeof(TEntity), Id, ViewModelAction.Delete));
            await DeleteEntityRelationshipAsync(cancellationToken);
        }

        public async Task<TPrimaryKey> SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                BeginUow();
                IsValid = Validator.TryValidateObject(this, ValidateContext, Errors);
                await Validate(cancellationToken);
                if (!IsValid)
                {
                    await HandleExceptionAsync(new MixException(MixErrorStatus.Badrequest, Errors.Select(e => e.ErrorMessage).ToArray()));
                }
                var entity = await SaveHandlerAsync(cancellationToken);
                await CompleteUowAsync(cancellationToken);
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

        public async Task<TPrimaryKey> SaveFieldsAsync(IEnumerable<EntityPropertyModel> properties, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
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
                IsValid = Validator.TryValidateObject(this, ValidateContext, Errors);
                await Validate(cancellationToken);
                var entity = await ParseEntity(cancellationToken);
                await Repository.SaveAsync(entity, cancellationToken);
                await CompleteUowAsync(cancellationToken);
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
        protected virtual async Task<TEntity> SaveHandlerAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entity = await ParseEntity(cancellationToken);

            ModifiedEntities.Add(new(typeof(TEntity), Id, !Id.Equals(default) ? ViewModelAction.Create : ViewModelAction.Update));

            await Repository.SaveAsync(entity, cancellationToken);
            await SaveEntityRelationshipAsync(entity, cancellationToken);
            Id = entity.Id;
            return entity;
        }

        // Override this method
        protected virtual Task SaveEntityRelationshipAsync(TEntity parentEntity, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        protected virtual Task DeleteEntityRelationshipAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}
