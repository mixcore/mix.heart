﻿// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Common.Helper;
using Mix.Domain.Core.ViewModels;
using Mix.Heart.Enums;
using Mix.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Mix.Heart.Extensions;
using Mix.Heart.Helpers;

namespace Mix.Domain.Data.Repository
{
    /// <summary>
    /// View Repository Base
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    public abstract class ViewRepositoryBase<TDbContext, TModel, TView>
        where TDbContext : DbContext
        where TModel : class
        where TView : ViewModels.ViewModelBase<TDbContext, TModel, TView>
    {
        #region Properties
        public string KeyName { get; set; } = "Id";
        public string ModelName { get { return typeof(TView).FullName; } }
        public bool IsCache
        {
            get { return CommonHelper.GetWebConfig<bool>("IsCache"); }
        }
        public string CachedFolder { get { return ModelName.Substring(0, ModelName.LastIndexOf('.')).Replace('.', '/'); } }
        public string CachedFileName { get { return typeof(TView).Name; } }

        public string[] SelectedMembers { get { return FilterSelectedFields(); } }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRepositoryBase{TDbContext, TModel, TView}"/> class.
        /// </summary>
        protected ViewRepositoryBase()
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
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
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
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
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
        public virtual RepositoryResponse<TView> CreateModel(TView view
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
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
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
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
                if (result.IsSucceed)
                {
                    RemoveCache(view.Model, context, transaction);
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
                //context.Entry(view.Model).State = EntityState.Modified;
                context.Set<TModel>().Update(view.Model);
                result.IsSucceed = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                result.Data = view;
                if (result.IsSucceed)
                {
                    _ = RemoveCache(view.Model, context, transaction);
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
                TModel model = context.Set<TModel>().SelectMembers(SelectedMembers).SingleOrDefault(predicate);
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
                TModel model = await context.Set<TModel>().SelectMembers(SelectedMembers).SingleOrDefaultAsync(predicate).ConfigureAwait(false);
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
                TModel model = context.Set<TModel>().SelectMembers(SelectedMembers).FirstOrDefault(predicate);
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
                TModel model = await context.Set<TModel>().SelectMembers(SelectedMembers).FirstOrDefaultAsync(predicate).ConfigureAwait(false);
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
            Console.WriteLine(ex);
        }

        /// <summary>
        /// Parses the paging query hronous.
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
        , string orderByPropertyName, MixHeartEnums.DisplayDirection direction
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
                var members = IsCache ? context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties.Select(x => x.Name).ToArray()
                                        : SelectedMembers;
                if (pageSize.HasValue && pageSize.Value > 0)
                {
                    result.TotalPage = (result.TotalItems / pageSize.Value) + (result.TotalItems % pageSize.Value > 0 ? 1 : 0);
                }

                switch (direction)
                {
                    case MixHeartEnums.DisplayDirection.Desc:
                        sorted = Queryable.OrderByDescending(query, orderBy);
                        if (pageSize.HasValue && pageSize.Value > 0)
                        {
                            lstModel = sorted.Skip(result.PageIndex * pageSize.Value)
                            .SelectMembers(members)
                            .Take(pageSize.Value)
                            .ToList();
                        }
                        else
                        {
                            if (top.HasValue)
                            {
                                lstModel = sorted.Skip(skip.HasValue ? skip.Value : 0)
                                    .SelectMembers(members)
                            .Take(top.Value)
                            .ToList();
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
                            lstModel = sorted
                                .Skip(result.PageIndex * pageSize.Value)
                                .SelectMembers(members)
                                .Take(pageSize.Value)
                                .ToList();
                        }
                        else
                        {
                            if (top.HasValue)
                            {
                                lstModel = sorted
                                    .Skip(skip.HasValue ? skip.Value : 0)
                                    .SelectMembers(members)
                                    .Take(top.Value)
                                    .ToList();
                            }
                            else
                            {
                                lstModel = sorted.SelectMembers(members).ToList();
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
        public virtual async Task<PaginationModel<TView>> ParsePagingQueryAsync(IQueryable<TModel> query
        , string orderByPropertyName, MixHeartEnums.DisplayDirection direction
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
                var members = IsCache ? context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties.Select(x => x.Name).ToArray()
                                        : SelectedMembers;
                if (pageSize.HasValue && pageSize.Value > 0)
                {
                    result.TotalPage = (result.TotalItems / pageSize.Value) + (result.TotalItems % pageSize.Value > 0 ? 1 : 0);
                }

                switch (direction)
                {
                    case MixHeartEnums.DisplayDirection.Desc:
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
                                lstModel = await sorted.Skip(skip.HasValue ? skip.Value : 0)
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
                                    .Skip(skip.HasValue ? skip.Value : 0)
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
                if (classConstructor != null)
                {
                    return (TView)classConstructor.Invoke(new object[] { model });
                }
                else
                {
                    classConstructor = classType.GetConstructor(new Type[] { model.GetType() });
                    return (TView)classConstructor.Invoke(new object[] { });
                }
            }
        }

        /// <summary>
        /// Registers the automatic mapper.
        /// </summary>
        public virtual void RegisterAutoMapper()
        {
            // TODO: Create mapper
            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<TModel, TView>();
            //    cfg.CreateMap<TView, TModel>();
            //});
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
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var lstModel = context.Set<TModel>().ToList();

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
        string orderByPropertyName, MixHeartEnums.DisplayDirection direction, int? pageSize, int? pageIndex
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);

            try
            {
                var query = context.Set<TModel>();

                var result = ParsePagingQuery(query, orderByPropertyName, direction, pageSize, pageIndex
                    , null, null
                    , context, transaction);

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
                var lstModel = await context.Set<TModel>().ToListAsync().ConfigureAwait(false);

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
        string orderByPropertyName, MixHeartEnums.DisplayDirection direction, int? pageSize, int? pageIndex, int? skip = null, int? top = null
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
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
                var members = IsCache ? context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties.Select(x => x.Name).ToArray()
                                       : SelectedMembers;
                var lstModel = context.Set<TModel>().Where(predicate).SelectMembers(members).ToList();
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
        Expression<Func<TModel, bool>> predicate, string orderByPropertyName, MixHeartEnums.DisplayDirection direction, int? pageSize, int? pageIndex
        , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            try
            {
                var query = context.Set<TModel>().Where(predicate);
                var result = ParsePagingQuery(query
                , orderByPropertyName, direction
                , pageSize, pageIndex
                , null, null
                , context, transaction);
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
                var lstModel = await context.Set<TModel>().Where(predicate).SelectMembers(members).ToListAsync();
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
        , MixHeartEnums.DisplayDirection direction, int? pageSize, int? pageIndex, int? skip = null, int? top = null
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
        /// Removes the model.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TModel> RemoveModel(Expression<Func<TModel, bool>> predicate, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            TModel model = context.Set<TModel>().FirstOrDefault(predicate);
            bool result = true;
            try
            {
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
                    _ = RemoveCache(model);
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
            bool result = true;
            try
            {

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
                    RemoveCache(model);
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
            TModel model = await context.Set<TModel>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
            bool result = true;
            try
            {
                if (model != null && CheckIsExists(model, context, transaction))
                {
                    context.Entry(model).State = EntityState.Deleted;
                    result = await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                    if (result)
                    {
                        _ = RemoveCache(model);
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
                    _ = RemoveCache(model);
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
                        _ = RemoveCache(model);
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
                    _ = RemoveCache(model);
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
            if (CheckIsExists(view.Model, _context, _transaction))
            {
                return EditModel(view, _context, _transaction);
            }
            else
            {
                return CreateModel(view, _context, _transaction);
            }
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
            try
            {
                int total = context.Set<TModel>().Any()
                   ? context.Set<TModel>().Max(predicate)
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
                int total = context.Set<TModel>().Any()
                    ? await context.Set<TModel>().MaxAsync(predicate)
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
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
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
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
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
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
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
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
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
            bool result = false;
            TModel model = context.Set<TModel>().FirstOrDefault(predicate);
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
                if (result)
                {
                    RemoveCache(model, context, transaction);
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
            bool result = false;
            TModel model = await context.Set<TModel>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
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
                    _ = RemoveCache(model, context, transaction);
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

        string[] FilterSelectedFields()
        {
            var viewProperties = typeof(TView).GetProperties();
            var modelProperties = typeof(TModel).GetProperties();
            return viewProperties.Where(p => modelProperties.Any(m => m.Name == p.Name)).Select(p => p.Name).ToArray();
        }


        #region Cached       
        public virtual TView GetCachedData(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (model != null)
            {
                string key = GetCachedKey(model, _context);
                string folder = $"{CachedFolder}/{key}";
                var data = CacheService.Get<TView>(CachedFileName, folder);
                if (data != null)
                {
                    data.ExpandView(_context, _transaction);
                    return data;
                }
                else
                {
                    var predicate = BuildExpressionByKeys(model, _context);
                    model = _context.Set<TModel>().FirstOrDefault(predicate);

                    data = ParseView(model, _context, _transaction);
                    if (data != null && data.IsCache)
                    {
                        Task.Run(() =>
                        {
                            CacheService.SetAsync(CachedFileName, data, folder);
                        });
                    }
                    return data;
                }
            }
            else
            {
                return null;
            }
        }

        private Expression<Func<TModel, bool>> BuildExpressionByKeys(TModel model, TDbContext context)
        {
            Expression<Func<TModel, bool>> predicate = null;
            foreach (var item in context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties)
            {
                var pre = ReflectionHelper.GetExpression<TModel>(
                        item.Name,
                        ReflectionHelper.GetPropertyValue(model, item.Name),
                        MixHeartEnums.ExpressionMethod.Eq);
                predicate = predicate == null ? pre
                    : ReflectionHelper.CombineExpression(predicate, pre, MixHeartEnums.ExpressionMethod.And);
            }
            return predicate;
        }

        public virtual List<TView> GetCachedData(List<TModel> models, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            List<TView> result = new List<TView>();
            foreach (var model in models)
            {
                TView data = GetCachedData(model, _context, _transaction);
                if (data != null)
                {
                    result.Add(data);
                }
            }
            return result;
        }
        public object GetPropValue(object src, string propName)
        {
            return src?.GetType().GetProperty(propName)?.GetValue(src, null);
        }
        public string GetCachedKey(TModel model, TDbContext _context)
        {
            var result = string.Empty;
            _context = _context ?? InitContext();
            var keys = _context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties
        .Select(x => x.Name);
            foreach (var key in keys)
            {
                result += $"_{GetPropValue(model, key)}";
            }
            return result;
        }
        public virtual Task AddToCache(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (model != null)
            {
                string key = GetCachedKey(model, _context);
                string folder = $"{CachedFolder}/{key}";
                var view = GetCachedData(model, _context, _transaction);
                CacheService.Set(CachedFileName, view, folder);
            }
            return Task.CompletedTask;
        }
        public virtual Task RemoveCache(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (model != null)
            {
                string key = GetCachedKey(model, _context);
                string folder = $"{CachedFolder}/{key}";
                CacheService.RemoveCacheAsync(folder);
            }
            return Task.CompletedTask;
        }

        public virtual Task RemoveCache(string key, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            string folder = $"{CachedFolder}/{key}";
            CacheService.RemoveCacheAsync(folder);
            return Task.CompletedTask;
        }
        #endregion
    }
}