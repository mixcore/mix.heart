// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Common.Helper;
using Mix.Heart.Enums;
using Mix.Heart.Extensions;
using Mix.Heart.Helpers;
using Mix.Heart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mix.Heart.Infrastructure.Repositories
{
    /// <summary>
    /// View Repository Base
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    public abstract partial class ViewRepositoryBase<TDbContext, TModel, TView>
    {

        /// <summary>
        /// Creates the model asynchronous.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TView>> CreateModelAsync(TView view
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TView> result = new RepositoryResponse<TView>() { IsSucceed = true };
            try
            {
                context.Entry(view.Model).State = EntityState.Added;
                result.IsSucceed = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                result.Data = view;
                if (result.IsSucceed)
                {
                    result.Data.ParseView(false, context, transaction);
                }

                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                return result;
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TView>(view, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Edits the model asynchronous.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TView>> EditModelAsync(TView view, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TView> result = new RepositoryResponse<TView>() { IsSucceed = true };
            try
            {
                context.Entry(view.Model).State = EntityState.Modified;
                result.IsSucceed = await context.SaveChangesAsync() > 0;
                result.Data = view;
                if (result.IsSucceed)
                {
                    await RemoveCacheAsync(view.Model, context, transaction);
                }
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                return result;
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TView>(view, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Gets the single model asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TView>> GetSingleModelAsync(
        Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                TModel model = await context.Set<TModel>().AsNoTracking().SelectMembers(SelectedMembers).SingleOrDefaultAsync(predicate).ConfigureAwait(false);
                if (model != null)
                {
                    context.Entry(model).State = EntityState.Detached;

                    return new RepositoryResponse<TView>()
                    {
                        IsSucceed = true,
                        Data = IsCache ? GetCachedData(model, context, transaction) : ParseView(model, context, transaction)
                    };
                }
                else
                {
                    return new RepositoryResponse<TView>()
                    {
                        IsSucceed = false,
                        Data = default
                    };
                }
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TView>(default, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Gets the single model asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TView>> GetFirstModelAsync(
        Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                TModel model = await context.Set<TModel>().AsNoTracking().SelectMembers(SelectedMembers).FirstOrDefaultAsync(predicate).ConfigureAwait(false);
                if (model != null)
                {
                    context.Entry(model).State = EntityState.Detached;

                    return new RepositoryResponse<TView>()
                    {
                        IsSucceed = true,
                        Data = IsCache ? GetCachedData(model, context, transaction) : ParseView(model, context, transaction)
                    };
                }
                else
                {
                    return new RepositoryResponse<TView>()
                    {
                        IsSucceed = false,
                        Data = default
                    };
                }
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TView>(default, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Parses the paging query asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="orderByPropertyName">Name of the order by property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="context">The context.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<PaginationModel<TView>> ParsePagingQueryAsync(IQueryable<TModel> query
        , string orderByPropertyName, DisplayDirection direction
        , int? pageSize, int? pageIndex, int? skip, int? top
        , TDbContext context, IDbContextTransaction transaction)
        {
            List<TModel> lstModel = new List<TModel>();

            PaginationModel<TView> result = new PaginationModel<TView>()
            {
                TotalItems = query.Count(),
                PageIndex = pageIndex ?? 0
            };
            dynamic orderBy = ReflectionHelper.GetLambda<TModel>(orderByPropertyName);
            IQueryable<TModel> sorted = null;
            try
            {
                result.PageSize = pageSize ?? result.TotalItems;
                var members = IsCache ? GetKeyMembers(context.Model)
                                        : SelectedMembers;
                if (pageSize.HasValue && pageSize.Value > 0)
                {
                    result.TotalPage = (result.TotalItems / pageSize.Value) + (result.TotalItems % pageSize.Value > 0 ? 1 : 0);
                }

                switch (direction)
                {
                    case DisplayDirection.Desc:
                        sorted = Queryable.OrderByDescending(query, orderBy);
                        if (pageSize.HasValue && pageSize.Value > 0)
                        {
                            lstModel = await sorted.Skip(result.PageIndex * pageSize.Value)
                            .SelectMembers(members)
                            .Take(pageSize.Value)
                            .ToListAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            if (top.HasValue)
                            {
                                lstModel = await sorted.Skip(skip ?? 0)
                                    .SelectMembers(members)
                            .Take(top.Value)
                            .ToListAsync().ConfigureAwait(false);
                            }
                            else
                            {
                                lstModel = sorted.ToList();
                            }
                        }
                        break;

                    default:
                        sorted = Queryable.OrderBy(query, orderBy);
                        if (pageSize.HasValue && pageSize.Value > 0)
                        {
                            lstModel = await sorted
                                .Skip(result.PageIndex * pageSize.Value)
                                .SelectMembers(members)
                                .Take(pageSize.Value)
                                .ToListAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            if (top.HasValue)
                            {
                                lstModel = await sorted
                                    .Skip(skip ?? 0)
                                    .SelectMembers(members)
                                    .Take(top.Value)
                                    .ToListAsync().ConfigureAwait(false);
                            }
                            else
                            {
                                lstModel = await sorted.SelectMembers(members).ToListAsync().ConfigureAwait(false);
                            }
                        }
                        break;
                }
                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                if (IsCache)
                {
                    var lstView = GetCachedData(lstModel, context, transaction);
                    result.Items = lstView;
                }
                else
                {
                    result.Items = ParseView(lstModel, context, transaction);
                }

                return result;
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                return null;
            }
        }

        /// <summary>
        /// Parses the paging query asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="orderByPropertyName">Name of the order by property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="context">The context.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<PaginationModel<TView>> ParsePagingSortedQueryAsync(IQueryable<TModel> sortedQuery
        , int? pageSize, int? pageIndex, int? skip, int? top
        , TDbContext context, IDbContextTransaction transaction)
        {
            List<TModel> lstModel = new List<TModel>();

            PaginationModel<TView> result = new PaginationModel<TView>()
            {
                TotalItems = sortedQuery.Count(),
                PageIndex = pageIndex ?? 0
            };
            try
            {
                result.PageSize = pageSize ?? result.TotalItems;
                var members = IsCache ? GetKeyMembers(context.Model)
                                        : SelectedMembers;
                if (pageSize.HasValue && pageSize.Value > 0)
                {
                    result.TotalPage = (result.TotalItems / pageSize.Value) + (result.TotalItems % pageSize.Value > 0 ? 1 : 0);
                }

                if (pageSize.HasValue && pageSize.Value > 0)
                {
                    lstModel = await sortedQuery.Skip(result.PageIndex * pageSize.Value)
                    .SelectMembers(members)
                    .Take(pageSize.Value)
                    .ToListAsync().ConfigureAwait(false);
                }
                else
                {
                    if (top.HasValue)
                    {
                        lstModel = await sortedQuery.Skip(skip ?? 0)
                            .SelectMembers(members)
                    .Take(top.Value)
                    .ToListAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        lstModel = sortedQuery.ToList();
                    }
                }
                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                if (IsCache)
                {
                    var lstView = GetCachedData(lstModel, context, transaction);
                    result.Items = lstView;
                }
                else
                {
                    result.Items = ParseView(lstModel, context, transaction);
                }

                return result;
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                return null;
            }
        }

        #region GetModelList

        /// <summary>
        /// Gets the model list asynchronous.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<List<TView>>> GetModelListAsync(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            List<TView> result = new List<TView>();
            try
            {
                var lstModel = await context.Set<TModel>().AsNoTracking().ToListAsync().ConfigureAwait(false);

                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                return new RepositoryResponse<List<TView>>()
                {
                    IsSucceed = true,
                    Data = IsCache ? GetCachedData(lstModel, context, transaction) : ParseView(lstModel, context, transaction)
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(default, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Gets the model list asynchronous.
        /// </summary>
        /// <param name="orderByPropertyName">Name of the order by property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<PaginationModel<TView>>> GetModelListAsync(
        string orderByPropertyName, DisplayDirection direction, int? pageSize, int? pageIndex, int? skip = null, int? top = null
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var query = context.Set<TModel>().AsNoTracking();

                var result = await ParsePagingQueryAsync(query, orderByPropertyName, direction, pageSize, pageIndex, skip, top, context, transaction).ConfigureAwait(false);
                return new RepositoryResponse<PaginationModel<TView>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TView>>(default, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        #endregion GetModelList

        #region GetModelListBy

        /// <summary>
        /// Gets the model list by asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<List<TView>>> GetModelListByAsync(Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);

            try
            {
                var members = IsCache ? context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties.Select(x => x.Name).ToArray()
                                      : SelectedMembers;
                var lstModel = await context.Set<TModel>().AsNoTracking().Where(predicate).SelectMembers(members).ToListAsync();
                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                return new RepositoryResponse<List<TView>>()
                {
                    IsSucceed = true,
                    Data = IsCache ? GetCachedData(lstModel, context, transaction) : ParseView(lstModel, context, transaction)
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(default, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Gets the model list by asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="orderByPropertyName">Name of the order by property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<PaginationModel<TView>>> GetModelListByAsync(
        Expression<Func<TModel, bool>> predicate, string orderByPropertyName
        , DisplayDirection direction, int? pageSize, int? pageIndex, int? skip = null, int? top = null
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var query = context.Set<TModel>().AsNoTracking().Where(predicate);
                var result = await ParsePagingQueryAsync(query
                , orderByPropertyName, direction
                , pageSize, pageIndex, skip, top
                , context, transaction).ConfigureAwait(false);
                return new RepositoryResponse<PaginationModel<TView>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TView>>(default, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        #endregion GetModelListBy

        // TODO: Should return return enum status code instead
        /// <summary>
        /// Removes the list model asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<List<TModel>>> RemoveListModelAsync(bool isRemoveRelatedModels, Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var getData = await GetModelListByAsync(predicate, context, transaction).ConfigureAwait(false);
                //context.Set<TModel>().AsNoTracking().Where(predicate).ToListAsync().ConfigureAwait(false);
                var result = new RepositoryResponse<List<TModel>>() { IsSucceed = true };
                if (getData.IsSucceed)
                {
                    foreach (var item in getData.Data)
                    {
                        if (isRemoveRelatedModels)
                        {
                            var removeRelatedResult = await item.RemoveRelatedModelsAsync(item, context, transaction).ConfigureAwait(false);
                            if (removeRelatedResult.IsSucceed)
                            {
                                var temp = await RemoveModelAsync(item.Model, context, transaction).ConfigureAwait(false);
                                if (!temp.IsSucceed)
                                {
                                    result.IsSucceed = false;
                                    result.Exception = temp.Exception;
                                    result.Errors = temp.Errors;
                                    break;
                                }
                            }
                            else
                            {
                                result.IsSucceed = false;
                                result.Errors.AddRange(removeRelatedResult.Errors);
                                result.Exception = removeRelatedResult.Exception;
                                break;
                            }
                        }
                        else
                        {
                            var temp = await RemoveModelAsync(item.Model, context, transaction).ConfigureAwait(false);
                            if (!temp.IsSucceed)
                            {
                                result.IsSucceed = false;
                                result.Exception = temp.Exception;
                                result.Errors = temp.Errors;
                                break;
                            }
                        }
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
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TModel>>(default, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        // TODO: Should return return enum status code instead
        /// <summary>
        /// Removes the model asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TModel>> RemoveModelAsync(Expression<Func<TModel, bool>> predicate, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            TModel model = await context.Set<TModel>().AsNoTracking().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
            bool result = true;
            try
            {
                if (model != null && CheckIsExists(model, context, transaction))
                {
                    context.Entry(model).State = EntityState.Deleted;
                    result = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                    if (result)
                    {
                        await RemoveCacheAsync(model);
                    }
                }

                UnitOfWorkHelper<TDbContext>.HandleTransaction(result, isRoot, transaction);

                return new RepositoryResponse<TModel>()
                {
                    IsSucceed = result,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(model, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
                if (result)
                {
                    await RemoveCacheAsync(model);
                }
            }
        }

        // TODO: Should return return enum status code instead
        /// <summary>
        /// Removes the model asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TModel>> RemoveModelAsync(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            var result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                if (model != null && CheckIsExists(model, context, transaction))
                {
                    context.Entry(model).State = EntityState.Deleted;
                    result.IsSucceed = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                    if (result.IsSucceed)
                    {
                        await RemoveCacheAsync(model);
                    }
                }

                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);

                return result;
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(model, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
                if (result.IsSucceed)
                {
                    await RemoveCacheAsync(model);
                }
            }
        }

        /// <summary>
        /// Saves the model asynchronous.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="isSaveSubModels">if set to <c>true</c> [is save sub models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual Task<RepositoryResponse<TView>> SaveModelAsync(TView view, bool isSaveSubModels = false
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (CheckIsExists(view.Model, _context, _transaction))
            {
                return EditModelAsync(view, _context, _transaction);
            }
            else
            {
                return CreateModelAsync(view, _context, _transaction);
            }
        }

        /// <summary>
        /// Saves the model asynchronous.
        /// </summary>
        /// <param name="data">The view.</param>
        /// <param name="isSaveSubModels">if set to <c>true</c> [is save sub models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<List<TView>>> SaveListModelAsync(List<TView> data, bool isSaveSubModels = false
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            var result = new RepositoryResponse<List<TView>>() { IsSucceed = true };
            try
            {
                foreach (var item in data)
                {
                    var saveResult = await item.SaveModelAsync(isSaveSubModels, context, transaction);
                    if (!saveResult.IsSucceed)
                    {
                        result.IsSucceed = false;
                        result.Exception = saveResult.Exception;
                        result.Errors = saveResult.Errors;
                        break;
                    }
                }
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);

                return result;
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(default, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Saves the sub model asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Task<bool> SaveSubModelAsync(TModel model, TDbContext context, IDbContextTransaction _transaction)
        {
            return Task.FromResult(true);
        }

        #region Max

        /// <summary>
        /// Maximums the asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<int>> MaxAsync(Expression<Func<TModel, int>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                int total = context.Set<TModel>().AsNoTracking().Any()
                    ? await context.Set<TModel>().AsNoTracking().MaxAsync(predicate)
                    : 0;
                return new RepositoryResponse<int>()
                {
                    IsSucceed = true,
                    Data = total
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleObjectException<int>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        #endregion Max

        #region Min

        /// <summary>
        /// Maximums the asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<T>> MinAsync<T>(
            Expression<Func<TModel, bool>> predicate, Expression<Func<TModel, T>> min
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                T total = await context.Set<TModel>().AsNoTracking().Where(predicate).MinAsync(min).ConfigureAwait(false);
                return new RepositoryResponse<T>()
                {
                    IsSucceed = true,
                    Data = total
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleObjectException<T>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        #endregion Min

        #region Count

        /// <summary>
        /// Counts the asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<int>> CountAsync(Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            int total = 0;
            try
            {
                total = await context.Set<TModel>().AsNoTracking().CountAsync(predicate).ConfigureAwait(false);
                return new RepositoryResponse<int>()
                {
                    IsSucceed = true,
                    Data = total
                };
            }
            catch (Exception ex)
            {
                UnitOfWorkHelper<TDbContext>.HandleException<List<TModel>>(default, ex, isRoot, transaction);
                return new RepositoryResponse<int>()
                {
                    IsSucceed = true,
                    Data = total
                };
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        /// <summary>
        /// Counts the asynchronous.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<int>> CountAsync(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                int total = await context.Set<TModel>().AsNoTracking().CountAsync().ConfigureAwait(false);
                return new RepositoryResponse<int>()
                {
                    IsSucceed = true,
                    Data = total
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleObjectException<int>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        #endregion Count

        #region Update Fields

        
        /// <summary>
        /// Updates the fields asynchronous.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public async Task<RepositoryResponse<TModel>> UpdateFieldsAsync(Expression<Func<TModel, bool>> predicate
        , List<EntityField> fields
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            bool result = false;
            TModel model = await context.Set<TModel>().AsNoTracking().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
            try
            {
                if (model != null)
                {
                    foreach (var field in fields)
                    {
                        var lamda = ReflectionHelper.GetLambda<TModel>(field.PropertyName, false);
                        if (lamda != null)
                        {
                            var prop = context.Entry(model).Property(field.PropertyName);
                            prop.CurrentValue = field.PropertyValue;
                            await context.SaveChangesAsync().ConfigureAwait(false);
                            result = true;
                        }
                        else
                        {
                            result = false;
                            break;
                        }
                    }
                }
                if (result)
                {
                    await RemoveCacheAsync(model, context, transaction);
                }
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result, isRoot, transaction);

                return new RepositoryResponse<TModel>
                {
                    IsSucceed = result,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(model, ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        #endregion Update Fields

    }
}