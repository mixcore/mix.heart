using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.Helpers;
using Mix.Heart.Models;
using System;
using System.Collections.Generic;
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

    protected virtual Task DeleteHandlerAsync(CancellationToken cancellationToken = default)
    {
        return Repository.DeleteAsync(Id, cancellationToken);
    }

    public async Task<TPrimaryKey> SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            BeginUow();
            await Validate();
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
        var entity = await ParseEntity();
        await Repository.SaveAsync(entity, cancellationToken);
        await SaveEntityRelationshipAsync(entity, cancellationToken);
        Id = entity.Id;
        return entity;
    }

    // Override this method
    protected virtual Task SaveEntityRelationshipAsync(TEntity parentEntity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion

    #endregion
}
}
