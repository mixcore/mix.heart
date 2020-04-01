// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Common.Helper;
using Mix.Domain.Core.ViewModels;
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
    /// <typeparam name="TModel">The type of the view.</typeparam>
    public abstract class ModelRepositoryBase<TDbContext, TModel>
        where TDbContext : DbContext
        where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelRepositoryBase{TDbContext, TModel}"/> class.
        /// </summary>
        protected ModelRepositoryBase()
        {
        }

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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TModel> CreateModel(TModel view
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            TDbContext context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            RepositoryResponse<TModel> result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                context.Entry(view).State = EntityState.Added;
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates the model asynchronous.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TModel>> CreateModelAsync(TModel view
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TModel> result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                context.Entry(view).State = EntityState.Added;
                result.IsSucceed = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                result.Data = view;
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
                    transaction.Dispose();
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual RepositoryResponse<TModel> EditModel(TModel view
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TModel> result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                //context.Entry(view).State = EntityState.Modified;
                context.Set<TModel>().Update(view);
                result.IsSucceed = context.SaveChanges() > 0;
                result.Data = view;
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
                    transaction.Dispose();
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual async Task<RepositoryResponse<TModel>> EditModelAsync(TModel view, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TModel> result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                //context.Entry(view).State = EntityState.Modified;
                context.Set<TModel>().Update(view);
                result.IsSucceed = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                result.Data = view;
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual RepositoryResponse<TModel> GetSingleModel(
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
                    context.Entry(model).State = EntityState.Detached;
                    return new RepositoryResponse<TModel>()
                    {
                        IsSucceed = true,
                        Data = model
                    };
                }
                else
                {
                    return new RepositoryResponse<TModel>()
                    {
                        IsSucceed = false,
                        Data = default
                    };
                }
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
                    context?.Dispose();
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
        public virtual async Task<RepositoryResponse<TModel>> GetSingleModelAsync(
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

                    return new RepositoryResponse<TModel>()
                    {
                        IsSucceed = true,
                        Data = model
                    };
                }
                else
                {
                    return new RepositoryResponse<TModel>()
                    {
                        IsSucceed = false,
                        Data = default
                    };
                }
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual RepositoryResponse<TModel> GetFirstModel(
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
                    return new RepositoryResponse<TModel>()
                    {
                        IsSucceed = true,
                        Data = model
                    };
                }
                else
                {
                    return new RepositoryResponse<TModel>()
                    {
                        IsSucceed = false,
                        Data = default
                    };
                }
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
                    context?.Dispose();
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
        public virtual async Task<RepositoryResponse<TModel>> GetFirstModelAsync(
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


                    return new RepositoryResponse<TModel>()
                    {
                        IsSucceed = true,
                        Data = model
                    };
                }
                else
                {
                    return new RepositoryResponse<TModel>()
                    {
                        IsSucceed = false,
                        Data = default
                    };
                }
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        }

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
        public virtual PaginationModel<TModel> ParsePagingQuery(IQueryable<TModel> query
        , string orderByPropertyName, int direction
        , int? pageSize, int? pageIndex
        , TDbContext context, IDbContextTransaction transaction)
        {
            List<TModel> lstModel = new List<TModel>();

            PaginationModel<TModel> result = new PaginationModel<TModel>()
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
                result.Items = lstModel;
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
        public virtual async Task<PaginationModel<TModel>> ParsePagingQueryAsync(IQueryable<TModel> query
        , string orderByPropertyName, int direction
        , int? pageSize, int? pageIndex
        , TDbContext context, IDbContextTransaction transaction)
        {
            List<TModel> lstModel = new List<TModel>();

            PaginationModel<TModel> result = new PaginationModel<TModel>()
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
                            lstModel = sorted.ToList();
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
                            lstModel = await sorted.ToListAsync().ConfigureAwait(false);
                        }
                        break;
                }
                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                result.Items = lstModel;
                return result;
            }
            catch (Exception ex)
            {
                LogErrorMessage(ex);
                return null;
            }
        }

        /// <summary>
        /// Registers the automatic mapper.
        /// </summary>
        public virtual void RegisterAutoMapper()
        {
            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<TModel, TModel>();
            //    cfg.CreateMap<TModel, TModel>();
            //});
        }

        #region GetModelList

        /// <summary>
        /// Gets the model list.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<List<TModel>> GetModelList(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            var context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            try
            {
                var lstModel = context.Set<TModel>().ToList();

                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                return new RepositoryResponse<List<TModel>>()
                {
                    IsSucceed = true,
                    Data = lstModel
                };
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual RepositoryResponse<PaginationModel<TModel>> GetModelList(
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

                return new RepositoryResponse<PaginationModel<TModel>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the model list asynchronous.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<List<TModel>>> GetModelListAsync(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            var context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();
            List<TModel> result = new List<TModel>();
            try
            {
                var lstModel = await context.Set<TModel>().ToListAsync().ConfigureAwait(false);

                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                return new RepositoryResponse<List<TModel>>()
                {
                    IsSucceed = true,
                    Data = lstModel
                };
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual async Task<RepositoryResponse<PaginationModel<TModel>>> GetModelListAsync(
        string orderByPropertyName, int direction, int? pageSize, int? pageIndex
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            bool isRoot = _context == null;
            var context = _context ?? InitContext();
            var transaction = _transaction ?? context.Database.BeginTransaction();

            try
            {
                var query = context.Set<TModel>();

                var result = await ParsePagingQueryAsync(query, orderByPropertyName, direction, pageSize, pageIndex, context, transaction).ConfigureAwait(false);
                return new RepositoryResponse<PaginationModel<TModel>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual RepositoryResponse<List<TModel>> GetModelListBy(Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var lstModel = context.Set<TModel>().Where(predicate).ToList();
                lstModel.ForEach(model => context.Entry(model).State = EntityState.Detached);
                return new RepositoryResponse<List<TModel>>()
                {
                    IsSucceed = true,
                    Data = lstModel
                };
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual RepositoryResponse<PaginationModel<TModel>> GetModelListBy(
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
                return new RepositoryResponse<PaginationModel<TModel>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual async Task<RepositoryResponse<List<TModel>>> GetModelListByAsync(Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);

            try
            {
                var query = context.Set<TModel>().Where(predicate);
                var result = await query.ToListAsync().ConfigureAwait(false);
                result.ForEach(model => context.Entry(model).State = EntityState.Detached);
                return new RepositoryResponse<List<TModel>>()
                {
                    IsSucceed = true,
                    Data = result
                };
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual async Task<RepositoryResponse<PaginationModel<TModel>>> GetModelListByAsync(
        Expression<Func<TModel, bool>> predicate, string orderByPropertyName
        , int direction, int? pageSize, int? pageIndex
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var query = context.Set<TModel>().Where(predicate);

                var result = await ParsePagingQueryAsync(query
                , orderByPropertyName, direction
                , pageSize, pageIndex
                , context, transaction).ConfigureAwait(false);
                return new RepositoryResponse<PaginationModel<TModel>>()
                {
                    IsSucceed = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<PaginationModel<TModel>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual RepositoryResponse<List<TModel>> RemoveListModel(Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var Items = context.Set<TModel>().Where(predicate).ToList();
                bool result = true;
                if (Items != null)
                {
                    foreach (var model in Items)
                    {
                        if (result)
                        {
                            var r = RemoveModel(model, context, transaction);
                            result = result && r.IsSucceed;
                        }
                        else
                        {
                            break;
                        }
                    }

                    UnitOfWorkHelper<TDbContext>.HandleTransaction(result, isRoot, transaction);

                    return new RepositoryResponse<List<TModel>>()
                    {
                        IsSucceed = result,
                        Data = Items
                    };
                }
                else
                {
                    return new RepositoryResponse<List<TModel>>()
                    {
                        IsSucceed = result,
                        Data = Items
                    };
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual async Task<RepositoryResponse<List<TModel>>> RemoveListModelAsync(Expression<Func<TModel, bool>> predicate
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var Items = await context.Set<TModel>().Where(predicate).ToListAsync().ConfigureAwait(false);
                bool result = true;
                if (Items != null)
                {
                    foreach (var model in Items)
                    {
                        if (result)
                        {
                            var r = await RemoveModelAsync(model, context, transaction).ConfigureAwait(false);
                            result = result && r.IsSucceed;
                        }
                        else
                        {
                            break;
                        }
                    }

                    UnitOfWorkHelper<TDbContext>.HandleTransaction(result, isRoot, transaction);

                    return new RepositoryResponse<List<TModel>>()
                    {
                        IsSucceed = result,
                        Data = Items
                    };
                }
                else
                {
                    return new RepositoryResponse<List<TModel>>()
                    {
                        IsSucceed = true,
                        Data = Items
                    };
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
        public virtual RepositoryResponse<TModel> SaveModel(TModel view, bool isSaveSubModels = false
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (CheckIsExists(view, _context, _transaction))
            {
                return EditModel(view, _context, _transaction);
            }
            else
            {
                return CreateModel(view, _context, _transaction);
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
        public virtual Task<RepositoryResponse<TModel>> SaveModelAsync(TModel view, bool isSaveSubModels = false
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (CheckIsExists(view, _context, _transaction))
            {
                return EditModelAsync(view, _context, _transaction);
            }
            else
            {
                return CreateModelAsync(view, _context, _transaction);
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
                    context.Database.CloseConnection();transaction.Dispose();context.Dispose();
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
    }
}