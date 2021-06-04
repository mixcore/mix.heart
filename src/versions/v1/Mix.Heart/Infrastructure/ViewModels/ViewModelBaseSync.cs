// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Common.Helper;
using Mix.Heart.Enums;
using Mix.Heart.Infrastructure.Interfaces;
using Mix.Heart.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
        #region Sync

        /// <summary>
        /// Clones the specified clone cultures.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cloneCultures">The clone cultures.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<List<TView>> Clone(TModel model, List<SupportedCulture> cloneCultures, TDbContext _context = null, IDbContextTransaction _transaction = null)
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

                        //TView view = InitView();
                        TModel m = (TModel)context.Entry(model).CurrentValues.ToObject();
                        Type myType = typeof(TModel);
                        var myFieldInfo = myType.GetProperty("Specificulture");
                        myFieldInfo.SetValue(m, desSpecificulture);
                        //view.Model = m;
                        //view.ParseView(isExpand: false, _context: context, _transaction: transaction);
                        bool isExist = Repository.CheckIsExists(m, _context: context, _transaction: transaction);

                        if (isExist)
                        {
                            result.IsSucceed = true;
                            //result.Data.Add(view);
                        }
                        else
                        {
                            context.Entry(m).State = EntityState.Added;
                            context.SaveChanges();
                            var cloneSubResult = CloneSubModels(m, cloneCultures, context, transaction);
                            if (!cloneSubResult.IsSucceed)
                            {
                                cloneSubResult.Errors.AddRange(cloneSubResult.Errors);
                                cloneSubResult.Exception = cloneSubResult.Exception;
                            }

                            result.IsSucceed = result.IsSucceed && cloneSubResult.IsSucceed && cloneSubResult.IsSucceed;
                            //result.Data.Add(cloneResult.Data);
                        }
                        UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                    }
                    return result;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Exception = ex;
                return result;
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
        /// Clones the sub models.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="cloneCultures">The clone cultures.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<bool> CloneSubModels(TModel parent, List<SupportedCulture> cloneCultures, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            return new RepositoryResponse<bool>() { IsSucceed = true };
        }

        /// <summary>
        /// Removes the model.
        /// </summary>
        /// <param name="isRemoveRelatedModels">if set to <c>true</c> [is remove related models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TModel> RemoveModel(bool isRemoveRelatedModels = false, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TModel> result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                ParseModel(_context, _transaction);
                if (isRemoveRelatedModels)
                {
                    var removeRelatedResult = RemoveRelatedModels((TView)this, context, transaction);
                    if (removeRelatedResult.IsSucceed)
                    {
                        result = Repository.RemoveModel(Model, context, transaction);
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
                    result = Repository.RemoveModel(Model, context, transaction);
                }

                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
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
                    RemoveCache(Model, context, transaction);
                }
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }                
            }
        }

        /// <summary>
        /// Removes the related models.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<bool> RemoveRelatedModels(TView view, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            return new RepositoryResponse<bool>() { IsSucceed = true };
        }

        /// <summary>
        /// Saves the model.
        /// </summary>
        /// <param name="isSaveSubModels">if set to <c>true</c> [is save sub models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TView> SaveModel(bool isSaveSubModels = false, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TView> result = new RepositoryResponse<TView>() { IsSucceed = true };
            Validate(context, transaction);
            if (IsValid)
            {
                try
                {
                    ParseModel(context, transaction);
                    result = Repository.SaveModel((TView)this, _context: context, _transaction: transaction);

                    // Save sub Models
                    if (result.IsSucceed && isSaveSubModels)
                    {
                        var saveResult = SaveSubModels(Model, context, transaction);
                        if (!saveResult.IsSucceed)
                        {
                            result.Errors.AddRange(saveResult.Errors);
                            result.Exception = saveResult.Exception;
                        }
                        result.IsSucceed = result.IsSucceed && saveResult.IsSucceed;
                    }

                    UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                    return result;
                }
                catch (Exception ex)
                {
                    return UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
                }
                finally
                {
                    if (result.IsSucceed && IsCache)
                    {
                        RemoveCache(Model, context, transaction).ConfigureAwait(true).GetAwaiter();
                    }
                    if (isRoot)
                    {
                        //if current Context is Root
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
        /// Saves the sub models.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<bool> SaveSubModels(TModel parent, TDbContext _context, IDbContextTransaction _transaction)
        {
            return new RepositoryResponse<bool>() { IsSucceed = true };
        }

        public virtual void SendNotify(object sender, RepositoryAction create, bool isSucceed)
        {
            if (this._mediator != null)
            {
                this._mediator.Notify(sender, RepositoryAction.Create, isSucceed);
            }
        }

        public void Notify(object sender, RepositoryAction action, bool isSucceed)
        {
        }
        #endregion Sync
    }
}