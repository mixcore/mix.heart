// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Common.Helper;
using Mix.Heart.Enums;
using Mix.Heart.Helpers;
using Mix.Heart.Infrastructure.Interfaces;
using Mix.Heart.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Mix.Heart.Infrastructure.ViewModels
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    ///
    public abstract partial class ViewModelBase<TDbContext, TModel, TView> : ISerializable, IMediator
        where TDbContext : DbContext
        where TModel : class
        where TView : ViewModelBase<TDbContext, TModel, TView> // instance of inherited
    {
        #region Async

        public async Task<RepositoryResponse<bool>> UpdateFieldsAsync(JObject fields)
        {
            var result = new RepositoryResponse<bool> { IsSucceed = true };
            try
            {
                foreach (JProperty field in fields.Properties())
                {
                    // check if field name is exist
                    var lamda = ReflectionHelper.GetLambda<TView>(field.Name);
                    if (lamda != null)
                    {
                        ReflectionHelper.SetPropertyValue(this, field);
                    }
                    else
                    {
                        result.IsSucceed = false;
                        result.Errors.Add($"{field.Name} is invalid");
                    }
                }
                if (result.IsSucceed)
                {
                    var saveResult = await this.SaveModelAsync(false);
                    ViewModelHelper.HandleResult(saveResult, ref result);
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Errors.Add(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Clones the asynchronous.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cloneCultures">The clone cultures.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<List<TView>>> CloneAsync(TModel model, List<SupportedCulture> cloneCultures
            , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<List<TView>> result = new RepositoryResponse<List<TView>>()
            {
                IsSucceed = true,
                Data = new List<TView>()
            };

            try
            {
                if (cloneCultures != null)
                {
                    foreach (var culture in cloneCultures)
                    {
                        string desSpecificulture = culture.Specificulture;

                        TModel m = (TModel)context.Entry(model).CurrentValues.ToObject();
                        Type myType = typeof(TModel);
                        var myFieldInfo = myType.GetProperty("Specificulture");
                        myFieldInfo.SetValue(m, desSpecificulture);
                        bool isExist = Repository.CheckIsExists(m, _context: context, _transaction: transaction);

                        if (isExist)
                        {
                            result.IsSucceed = true;
                        }
                        else
                        {
                            context.Entry(m).State = EntityState.Added;
                            await context.SaveChangesAsync();
                            var cloneSubResult = await CloneSubModelsAsync(m, cloneCultures, context, transaction).ConfigureAwait(false);
                            if (!cloneSubResult.IsSucceed)
                            {
                                cloneSubResult.Errors.AddRange(cloneSubResult.Errors);
                                cloneSubResult.Exception = cloneSubResult.Exception;
                            }

                            result.IsSucceed = result.IsSucceed && cloneSubResult.IsSucceed && cloneSubResult.IsSucceed;
                        }
                        _ = this.SendNotifyAsync(this, RepositoryAction.Clone, result.IsSucceed);
                    }
                    UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                    return result;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Clones the sub models asynchronous.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="cloneCultures">The clone cultures.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<bool>> CloneSubModelsAsync(TModel parent, List<SupportedCulture> cloneCultures, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            var taskSource = new TaskCompletionSource<RepositoryResponse<bool>>();
            taskSource.SetResult(new RepositoryResponse<bool>() { IsSucceed = true, Data = true });
            return await taskSource.Task;
        }

        /// <summary>
        /// Removes the model asynchronous.
        /// </summary>
        /// <param name="isRemoveRelatedModels">if set to <c>true</c> [is remove related models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TModel>> RemoveModelAsync(bool isRemoveRelatedModels = false, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);

            RepositoryResponse<TModel> result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                ParseModel(_context, _transaction);
                if (isRemoveRelatedModels)
                {
                    var removeRelatedResult = await RemoveRelatedModelsAsync((TView)this, context, transaction).ConfigureAwait(false);
                    if (removeRelatedResult.IsSucceed)
                    {
                        result = await Repository.RemoveModelAsync(Model, context, transaction).ConfigureAwait(false);
                    }
                    else
                    {
                        result.IsSucceed = result.IsSucceed && removeRelatedResult.IsSucceed;
                        result.Errors.AddRange(removeRelatedResult.Errors);
                        result.Exception = removeRelatedResult.Exception;
                    }
                }
                else
                {
                    result = await Repository.RemoveModelAsync(Model, context, transaction).ConfigureAwait(false);
                }
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                _ = this.SendNotifyAsync(this, RepositoryAction.Delete, result.IsSucceed);
                return result;
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(ex, isRoot, transaction);
            }
            finally
            {
                if (result.IsSucceed && IsCache)
                {
                    await RemoveCache(Model, context, transaction);
                }
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Removes the related models asynchronous.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<bool>> RemoveRelatedModelsAsync(TView view, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            var taskSource = new TaskCompletionSource<RepositoryResponse<bool>>();
            taskSource.SetResult(new RepositoryResponse<bool>() { IsSucceed = true });
            return await taskSource.Task;
        }

        /// <summary>
        /// Saves the model asynchronous.
        /// </summary>
        /// <param name="isSaveSubModels">if set to <c>true</c> [is save sub models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TView>> SaveModelAsync(bool isSaveSubModels = false, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TView> result = new RepositoryResponse<TView>() { IsSucceed = true };
            Validate(context, transaction);
            if (IsValid)
            {
                try
                {
                    ParseModel(context, transaction);
                    result = await Repository.SaveModelAsync((TView)this, _context: context, _transaction: transaction);

                    // Save sub Models
                    if (result.IsSucceed && isSaveSubModels)
                    {
                        var saveResult = await SaveSubModelsAsync(Model, context, transaction);
                        if (!saveResult.IsSucceed)
                        {
                            result.Errors.AddRange(saveResult.Errors);
                            result.Exception = saveResult.Exception;
                        }
                        result.IsSucceed = result.IsSucceed && saveResult.IsSucceed;
                    }

                    UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                    await this.SendNotifyAsync(this, RepositoryAction.Save, result.IsSucceed);
                    return result;
                }
                catch (Exception ex)
                {
                    return UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
                }
                finally
                {
                    if (isRoot)
                    {
                        if (result.IsSucceed && IsCache)
                        {
                            await RemoveCache(Model, context, transaction);
                        }
                        UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                    }
                }
            }
            else
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
                return new RepositoryResponse<TView>()
                {
                    IsSucceed = false,
                    Data = null,
                    Errors = Errors
                };
            }
        }

        /// <summary>
        /// Saves the sub models asynchronous.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<bool>> SaveSubModelsAsync(TModel parent, TDbContext _context, IDbContextTransaction _transaction)
        {
            var taskSource = new TaskCompletionSource<RepositoryResponse<bool>>();
            taskSource.SetResult(new RepositoryResponse<bool>() { IsSucceed = true });
            return await taskSource.Task;
        }

        public virtual Task SendNotifyAsync(object sender, RepositoryAction action, bool isSucceed)
        {
            return this._mediator != null
                ? this._mediator.NotifyAsync(sender, action, isSucceed)
                : Task.CompletedTask;
        }

        public virtual Task NotifyAsync(object sender, RepositoryAction action, bool isSucceed)
        {
            return Task.CompletedTask;
        }
        #endregion Async
    }
}