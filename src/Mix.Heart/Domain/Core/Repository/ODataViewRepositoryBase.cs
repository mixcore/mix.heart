// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Common.Helper;
using Mix.Domain.Core.ViewModels;
using Mix.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Mix.Domain.Data.Repository
{
    /// <summary>
    /// View Repository Base
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    public abstract class ODataViewRepositoryBase<TDbContext, TModel, TView>
        where TDbContext : DbContext
        where TModel : class
        where TView : ViewModels.ODataViewModelBase<TDbContext, TModel, TView>
    {
        #region Properties
        public string ModelName { get { return typeof(TView).FullName; } }
        public string CachedFolder { get { return ModelName.Substring(0, ModelName.LastIndexOf('.')).Replace('.', '/'); } }
        public string CachedFileName { get { return typeof(TView).Name; } }
        #endregion
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataViewRepositoryBase{TDbContext, TModel, TView}"/> class.
        /// </summary>
        protected ODataViewRepositoryBase()
        {
        }

        #region Common

        /// <summary>
        /// Checks the is exists.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual bool CheckIsExists(TModel entity, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                //For the former case use:
                return context.Set<TModel>().Any(e => e == entity);
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                if (isRoot)
                {
                    transaction.Rollback();
                }
                return false;
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    transaction.Dispose();
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks the is exists.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public bool CheckIsExists(System.Func<TModel, bool> predicate, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                //For the former case use:
                return context.Set<TModel>().Any(predicate);
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                if (isRoot)
                {
                    transaction.Rollback();
                }
                return false;
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    transaction.Dispose();
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Initializes the context.
        /// </summary>
        /// <returns></returns>
        public virtual TDbContext InitContext()
        {
            Type classType = typeof(TDbContext);
            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
            TDbContext context = (TDbContext)classConstructor.Invoke(new object[] { });

            return context;
        }

        /// <summary>
        /// Logs the error message.		User.Claims.ToList()	error CS0103: The name 'User' does not exist in the current context
        /// </summary>
        /// <param name="ex">The ex.</param>
        public virtual void LogErrorMessage(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        /// <summary>
        /// Parses the view.
        /// </summary>
        /// <param name="lstModels">The LST models.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual List<TView> ParseView(List<TModel> lstModels, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            List<TView> lstView = new List<TView>();
            foreach (var model in lstModels)
            {
                lstView.Add(ParseView(model, _context, _transaction));
            }

            return lstView;
        }

        /// <summary>
        /// Parses the view.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual TView ParseView(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            Type classType = typeof(TView);

            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { model.GetType(), typeof(TDbContext), typeof(IDbContextTransaction) });
            if (classConstructor != null)
            {
                return (TView)classConstructor.Invoke(new object[] { model, _context, _transaction });
            }
            else
            {
                classConstructor = classType.GetConstructor(new Type[] { model.GetType() });
                return (TView)classConstructor.Invoke(new object[] { model });
            }
        }

        #endregion

        #region Async

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
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
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
                    //if current Context is Root
                    transaction.Dispose();
                    context.Dispose();
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
                //context.Entry(view.Model).State = EntityState.Modified;
                
                context.Set<TModel>().Update(view.Model);
                result.IsSucceed = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                result.Data = view;
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);                
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
                    //if current Context is Root
                    context.Dispose();
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
                TModel model = await context.Set<TModel>().SingleOrDefaultAsync(predicate).ConfigureAwait(false);
                if (model != null)
                {
                    context.Entry(model).State = EntityState.Detached;

                    //var viewResult = ParseView(model, context, transaction);
                    return new RepositoryResponse<TView>()
                    {
                        IsSucceed = true,
                        Data = GetCachedData(model, context, transaction)
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
                return UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
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
                TModel model = await context.Set<TModel>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
                if (model != null)
                {
                    context.Entry(model).State = EntityState.Detached;

                    var viewResult = ParseView(model, context, transaction);
                    return new RepositoryResponse<TView>()
                    {
                        IsSucceed = true,
                        Data = viewResult
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
                return UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
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
        , string orderByPropertyName, int direction
        , int? pageSize, int? pageIndex, int? skip, int? top
        , TDbContext context, IDbContextTransaction transaction)
        {
            List<TModel> lstModel = new List<TModel>();

            PaginationModel<TView> result = new PaginationModel<TView>()
            {
                TotalItems = query.Count(),
                PageIndex = pageIndex ?? 0
            };
            dynamic orderBy = GetLambda(orderByPropertyName);
            IQueryable<TModel> sorted = null;
            try
            {
                result.PageSize = pageSize ?? result.TotalItems;

                if (pageSize.HasValue)
                {
                    result.TotalPage = (result.TotalItems / pageSize.Value) + (result.TotalItems % pageSize.Value > 0 ? 1 : 0);
                }

                switch (direction)
                {
                    case 1:
                        sorted = Queryable.OrderByDescending(query, orderBy);
                        if (pageSize.HasValue)
                        {
                            lstModel = await sorted.Skip(pageIndex.Value * pageSize.Value)
                            .Take(pageSize.Value)
                            .ToListAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            if (top.HasValue)
                            {
                                lstModel = await sorted.Skip(skip.HasValue ? skip.Value : 0)
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
                        if (pageSize.HasValue)
                        {
                            lstModel = await sorted
                                .Skip(pageIndex.Value * pageSize.Value)
                                .Take(pageSize.Value)
                                .ToListAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            if (top.HasValue)
                            {
                                lstModel = await sorted
                                    .Skip(skip.HasValue ? skip.Value : 0)
                                    .Take(top.Value)
                                    .ToListAsync().ConfigureAwait(false);
                            }
                            else
                            {
                                lstModel = await sorted.ToListAsync().ConfigureAwait(false);
                            }

                        }
                        break;
                }
                lstModel.ForEach(model =>
                {
                    context.Entry(model).State = EntityState.Detached;
                });
                var lstView = GetCachedData(lstModel, context, transaction);
                result.Items = lstView;
                return result;
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                return null;
            }
        }

        #endregion

        #region Sync
        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TView> CreateModel(TView view
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            TDbContext context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            RepositoryResponse<TView> result = new RepositoryResponse<TView>() { IsSucceed = true };
            try
            {
                context.Entry(view.Model).State = EntityState.Added;
                result.IsSucceed = context.SaveChanges() > 0;
                result.Data = view;
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                return result;
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                result.IsSucceed = false;
                result.Exception = ex;
                if (isRoot)
                {
                    transaction.Rollback();
                }
                return result;
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    transaction.Dispose();
                    context.Dispose();
                }
            }
        }



        /// <summary>
        /// Edits the model.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TView> EditModel(TView view
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TView> result = new RepositoryResponse<TView>() { IsSucceed = true };
            try
            {
                //context.Entry(view.Model).State = EntityState.Modified;
                context.Set<TModel>().Update(view.Model);
                result.IsSucceed = context.SaveChanges() > 0;
                result.Data = view;
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);              
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
                    //if current Context is Root
                    transaction.Dispose();
                    context.Dispose();
                }
            }
        }


        /// <summary>
        /// Gets the single model.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TView> GetSingleModel(
        Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                context = _context ?? InitContext();
                transaction = _transaction ?? context.Database.BeginTransaction();

                TModel model = context.Set<TModel>().SingleOrDefault(predicate);
                if (model != null)
                {
                    //context.Entry(model).State = EntityState.Detached;
                    //var viewResult = ParseView(model, context, transaction);

                    return new RepositoryResponse<TView>()
                    {
                        IsSucceed = true,
                        Data = GetCachedData(model, context, transaction)
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
                return UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context?.Dispose();
                }
            }
        }
        /// <summary>
        /// Gets the single model.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TView> GetFirstModel(
        Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                context = _context ?? InitContext();
                transaction = _transaction ?? context.Database.BeginTransaction();

                TModel model = context.Set<TModel>().FirstOrDefault(predicate);
                if (model != null)
                {
                    context.Entry(model).State = EntityState.Detached;
                    var viewResult = ParseView(model, context, transaction);
                    return new RepositoryResponse<TView>()
                    {
                        IsSucceed = true,
                        Data = viewResult
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
                return UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context?.Dispose();
                }
            }
        }

        /// <summary>
        /// Saves the model.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="isSaveSubModels">if set to <c>true</c> [is save sub models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TView> SaveModel(TView view, bool isSaveSubModels = false
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            RepositoryResponse<TView> result = null;
            if (CheckIsExists(view.Model, _context, _transaction))
            {
                result = EditModel(view, _context, _transaction);
            }
            else
            {
                result = CreateModel(view, _context, _transaction);
            }
            return result;
        }

        /// <summary>
        /// Saves the model hronous.
        /// </summary>
        /// <param name="data">The view.</param>
        /// <param name="isSaveSubModels">if set to <c>true</c> [is save sub models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<List<TView>> SaveListModel(List<TView> data, bool isSaveSubModels = false
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            var result = new RepositoryResponse<List<TView>>() { IsSucceed = true };
            try
            {
                foreach (var item in data)
                {
                    var saveResult = item.SaveModel(isSaveSubModels, context, transaction);
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
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        #endregion

        /// <summary>
        /// Parses the paging query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="orderByPropertyName">Name of the order by property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="context">The context.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public virtual PaginationModel<TView> ParsePagingQuery(IQueryable<TModel> query
        , string orderByPropertyName, int direction
        , int? pageSize, int? pageIndex
        , TDbContext context, IDbContextTransaction transaction)
        {
            List<TModel> lstModel = new List<TModel>();

            PaginationModel<TView> result = new PaginationModel<TView>()
            {
                TotalItems = query.Count(),
                PageIndex = pageIndex ?? 0
            };
            dynamic orderBy = GetLambda(orderByPropertyName);
            IQueryable<TModel> sorted = null;
            try
            {
                result.PageSize = pageSize > 0 ? pageSize : result.TotalItems;

                if (pageSize > 0)
                {
                    result.TotalPage = (result.TotalItems / pageSize.Value) + (result.TotalItems % pageSize.Value > 0 ? 1 : 0);
                }

                switch (direction)
                {
                    case 1:
                        sorted = Queryable.OrderByDescending(query, orderBy);
                        if (pageSize.HasValue)
                        {
                            lstModel = sorted.Skip(pageIndex.Value * pageSize.Value)
                            .Take(pageSize.Value)
                            .ToList();
                        }
                        else
                        {
                            lstModel = sorted.ToList();
                        }
                        break;

                    default:
                        sorted = Queryable.OrderBy(query, orderBy);
                        if (pageSize.HasValue)
                        {
                            lstModel = sorted
                            .Skip(pageIndex.Value * pageSize.Value)
                            .Take(pageSize.Value)
                            .ToList();
                        }
                        else
                        {
                            lstModel = sorted.ToList();
                        }
                        break;
                }
                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                var lstView = GetCachedData(lstModel, context, transaction);
                result.Items = lstView;
                return result;
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                return null;
            }
        }

        public string GetCachedKey(TModel model)
        {
            return $"{GetPropValue(model, "Specificulture")}_{GetPropValue(model, model.GetType().GetProperties()[0].Name)}";
        }

        public object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName)?.GetValue(src, null);
        }
        /// <summary>
        /// Registers the automatic mapper.
        /// </summary>
        public virtual void RegisterAutoMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<TModel, TView>();
                cfg.CreateMap<TView, TModel>();
            });
        }

        #region GetModelList

        /// <summary>
        /// Gets the model list.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<List<TView>> GetModelList(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            var context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            List<TView> result;
            try
            {
                var lstModel = context.Set<TModel>().ToList();

                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                result = ParseView(lstModel, context, transaction);
                return new RepositoryResponse<List<TView>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the model list.
        /// </summary>
        /// <param name="orderByPropertyName">Name of the order by property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<PaginationModel<TView>> GetModelList(
        string orderByPropertyName, int direction, int? pageSize, int? pageIndex
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            var context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();

            try
            {
                var query = context.Set<TModel>();

                var result = ParsePagingQuery(query, orderByPropertyName, direction, pageSize, pageIndex
                , context, transaction);

                return new RepositoryResponse<PaginationModel<TView>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the model list asynchronous.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<List<TView>>> GetModelListAsync(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            var context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            List<TView> result = new List<TView>();
            try
            {
                var lstModel = await context.Set<TModel>().ToListAsync().ConfigureAwait(false);

                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                result = ParseView(lstModel, _context, _transaction);
                return new RepositoryResponse<List<TView>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
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
        string orderByPropertyName, int direction, int? pageSize, int? pageIndex, int? skip = null, int? top = null
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            var context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();

            try
            {
                var query = context.Set<TModel>();

                var result = await ParsePagingQueryAsync(query, orderByPropertyName, direction, pageSize, pageIndex, skip, top, context, transaction).ConfigureAwait(false);
                return new RepositoryResponse<PaginationModel<TView>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        #endregion GetModelList

        #region GetModelListBy

        /// <summary>
        /// Gets the model list by.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<List<TView>> GetModelListBy(Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var lstModel = context.Set<TModel>().Where(predicate).ToList();
                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                var lstViewResult = ParseView(lstModel, _context, _transaction);
                return new RepositoryResponse<List<TView>>()
                {
                    IsSucceed = true,
                    Data = lstViewResult
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the model list by.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="orderByPropertyName">Name of the order by property.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<PaginationModel<TView>> GetModelListBy(
        Expression<Func<TModel, bool>> predicate, string orderByPropertyName, int direction, int? pageSize, int? pageIndex
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var query = context.Set<TModel>().Where(predicate);
                var result = ParsePagingQuery(query
                , orderByPropertyName, direction
                , pageSize, pageIndex
                , context, transaction);
                return new RepositoryResponse<PaginationModel<TView>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

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
                var query = context.Set<TModel>().Where(predicate);
                var lstModel = await query.ToListAsync().ConfigureAwait(false);
                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                var result = ParseView(lstModel, _context, _transaction);
                return new RepositoryResponse<List<TView>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
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
        , int direction, int? pageSize, int? pageIndex, int? skip = null, int? top = null
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var query = context.Set<TModel>().Where(predicate);

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
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        #endregion GetModelListBy

        // TODO: Should return return enum status code instead
        /// <summary>
        /// Removes the list model.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<List<TModel>> RemoveListModel(bool isRemoveRelatedModels, Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var getData = GetModelListBy(predicate, context, transaction);
                //context.Set<TModel>().Where(predicate).ToList().Configure(false);
                var result = new RepositoryResponse<List<TModel>>() { IsSucceed = true };
                if (getData.IsSucceed)
                {
                    foreach (var item in getData.Data)
                    {
                        if (isRemoveRelatedModels)
                        {
                            var removeRelatedResult = item.RemoveRelatedModels(item, context, transaction);
                            if (removeRelatedResult.IsSucceed)
                            {
                                var temp = RemoveModel(item.Model, context, transaction);
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
                            var temp = RemoveModel(item.Model, context, transaction);
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
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

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
                //context.Set<TModel>().Where(predicate).ToListAsync().ConfigureAwait(false);
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
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        // TODO: Should return return enum status code instead
        /// <summary>
        /// Removes the model.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TModel> RemoveModel(Expression<Func<TModel, bool>> predicate, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                TModel model = context.Set<TModel>().FirstOrDefault(predicate);
                bool result = true;
                if (model != null && CheckIsExists(model, context, transaction))
                {
                    context.Entry(model).State = EntityState.Deleted;
                    result = context.SaveChanges() > 0;
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
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        // TODO: Should return return enum status code instead
        /// <summary>
        /// Removes the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TModel> RemoveModel(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                bool result = true;
                if (model != null && CheckIsExists(model, context, transaction))
                {
                    context.Entry(model).State = EntityState.Deleted;
                    result = context.SaveChanges() > 0;
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
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
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
            try
            {
                TModel model = await context.Set<TModel>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
                bool result = true;
                if (model != null && CheckIsExists(model, context, transaction))
                {
                    context.Entry(model).State = EntityState.Deleted;
                    result = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
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
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
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
            try
            {
                var result = new RepositoryResponse<TModel>() { IsSucceed = true };
                if (model != null && CheckIsExists(model, context, transaction))
                {
                    context.Entry(model).State = EntityState.Deleted;
                    result.IsSucceed = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
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
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
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
        public virtual async Task<RepositoryResponse<TView>> SaveModelAsync(TView view, bool isSaveSubModels = false
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            RepositoryResponse<TView> result = null;
            if (CheckIsExists(view.Model, _context, _transaction))
            {
                result = await EditModelAsync(view, _context, _transaction);
            }
            else
            {
                result = await CreateModelAsync(view, _context, _transaction);
            }
            return result;
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
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
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
            throw new NotImplementedException();
        }

        #region Max

        /// <summary>
        /// Maximums the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<int> Max(Expression<Func<TModel, int>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            int total = 0;
            var result = new RepositoryResponse<int>()
            {
                IsSucceed = true,
                Data = total
            };
            try
            {
                total = context.Set<TModel>().Max(predicate);
                result.Data = total;
                return result;
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
                    context.Dispose();
                }
            }
        }

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
            int total = 0;
            try
            {
                total = await context.Set<TModel>().MaxAsync(predicate).ConfigureAwait(false);
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
                    context.Dispose();
                }
            }
        }

        #endregion Max
        #region Min

        /// <summary>
        /// Maximums the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<T> Min<T>(
            Expression<Func<TModel, bool>> predicate, Expression<Func<TModel, T>> min
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            T total = default(T);
            var result = new RepositoryResponse<T>()
            {
                IsSucceed = true,
                Data = total
            };
            try
            {
                total = context.Set<TModel>().Where(predicate).Min(min);
                result.Data = total;
                return result;
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
                    context.Dispose();
                }
            }
        }

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
            T total = default(T);
            try
            {
                total = await context.Set<TModel>().Where(predicate).MinAsync(min).ConfigureAwait(false);
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
                    context.Dispose();
                }
            }
        }
        #endregion

        #region Count

        /// <summary>
        /// Counts the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<int> Count(Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            int total = 0;
            try
            {
                total = context.Set<TModel>().Count(predicate);
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
                    context.Dispose();
                }
            }
        }

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
                total = await context.Set<TModel>().CountAsync(predicate).ConfigureAwait(false);
                return new RepositoryResponse<int>()
                {
                    IsSucceed = true,
                    Data = total
                };
            }
            catch (Exception ex)
            {
                UnitOfWorkHelper<TDbContext>.HandleException<List<TModel>>(ex, isRoot, transaction);
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
                    context.Dispose();
                }
            }
        }

        #endregion Count

        #region Count

        /// <summary>
        /// Counts the specified context.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<int> Count(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            int total = 0;
            try
            {
                total = context.Set<TModel>().Count();
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
                    context.Dispose();
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
            int total = 0;
            try
            {
                total = await context.Set<TModel>().CountAsync().ConfigureAwait(false);
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
                    context.Dispose();
                }
            }
        }

        #endregion Count

        #region Update Fields

        /// <summary>
        /// Updates the fields.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public RepositoryResponse<TModel> UpdateFields(Expression<Func<TModel, bool>> predicate
        , List<EntityField> fields
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                bool result = false;
                TModel model = context.Set<TModel>().FirstOrDefault(predicate);
                if (model != null)
                {
                    foreach (var field in fields)
                    {
                        var lamda = GetLambda(field.PropertyName, false);
                        if (lamda != null)
                        {
                            var prop = context.Entry(model).Property(field.PropertyName);
                            prop.CurrentValue = field.PropertyValue;

                            context.SaveChanges();
                            result = true;
                        }
                        else
                        {
                            result = false;
                            break;
                        }
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
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

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
            try
            {
                bool result = false;
                TModel model = await context.Set<TModel>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
                if (model != null)
                {
                    foreach (var field in fields)
                    {
                        var lamda = GetLambda(field.PropertyName, false);
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

                UnitOfWorkHelper<TDbContext>.HandleTransaction(result, isRoot, transaction);

                return new RepositoryResponse<TModel>
                {
                    IsSucceed = result,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        #endregion Update Fields

        /// <summary>
        /// Gets the lambda.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        /// <param name="isGetDefault">if set to <c>true</c> [is get default].</param>
        /// <returns></returns>
        protected LambdaExpression GetLambda(string propName, bool isGetDefault = true)
        {
            var parameter = Expression.Parameter(typeof(TModel));
            var type = typeof(TModel);
            var prop = Array.Find(type.GetProperties(), p => p.Name == propName);
            if (prop == null && isGetDefault)
            {
                propName = type.GetProperties().FirstOrDefault()?.Name;
            }
            var memberExpression = Expression.Property(parameter, propName);
            return Expression.Lambda(memberExpression, parameter);
        }

        #region Cached       
        public virtual TView GetCachedData(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            string key = GetCachedKey(model);
            string folder = $"{CachedFolder}/{key}";
            var data = CacheService.Get<TView>(CachedFileName, folder);
            if (data != null)
            {
                return data;
            }
            else
            {
                data = ParseView(model, _context, _transaction);
                _ = CacheService.SetAsync(CachedFileName, data, folder);
                return data;
            }
        }
        public virtual List<TView> GetCachedData(List<TModel> models, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            List<TView> result = new List<TView>();
            foreach (var model in models)
            {
                string key = GetCachedKey(model);
                string folder = $"{CachedFolder}/{key}";
                TView data = CacheService.Get<TView>(CachedFileName, folder);
                if (data != null)
                {
                    result.Add(data);
                }
                else
                {
                    data = ParseView(model, _context, _transaction);
                    if (data != null)
                    {
                        _ = CacheService.SetAsync(CachedFileName, data, folder);
                        result.Add(data);
                    }
                }
            }
            return result;
        }

        private Task AddToCache(TView view)
        {
            string key = GetCachedKey(view.Model);
            string folder = $"{CachedFolder}/{key}";
            CacheService.Set(CachedFileName, view, folder);
            return Task.CompletedTask;
        }
        private Task RemoveCache(TModel model)
        {
            string key = GetCachedKey(model);
            string folder = $"{CachedFolder}/{key}";
            CacheService.RemoveCacheAsync(folder);
            return Task.CompletedTask;
        }
        #endregion
    }
}