// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Common.Helper;
using Mix.Heart.Constants;
using Mix.Heart.Enums;
using Mix.Heart.Extensions;
using Mix.Heart.Helpers;
using Mix.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        where TDbContext : DbContext
        where TModel : class
        where TView : ViewModels.ViewModelBase<TDbContext, TModel, TView>
    {
        #region Properties

        public string KeyName { get; set; } = "Id";
        public string ModelName { get { return typeof(TModel).FullName; } }

        public bool IsCache { get => MixCommonHelper.GetWebConfig<bool>(WebConfiguration.IsCache); }

        public string CachedFolder { get { return $"{ModelName}"; } }

        public string[] SelectedMembers { get { return FilterSelectedFields(); } }

        #endregion Properties

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
                return context.Set<TModel>().AsNoTracking().Any(e => e == entity);
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
                return context.Set<TModel>().AsNoTracking().Any(predicate);
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

        private string[] FilterSelectedFields()
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
                var data = MixCacheService.Get<TView>(GetCacheFileName(model), folder);
                if (data != null)
                {
                    data.ExpandView(_context, _transaction);
                    return data;
                }
                else
                {
                    var predicate = BuildExpressionByKeys(model, _context);
                    model = _context.Set<TModel>().AsNoTracking().FirstOrDefault(predicate);
                    key = GetCachedKey(model, _context);
                    data = ParseView(model, _context, _transaction);
                    if (data != null && IsCache)
                    {
                        MixCacheService.SetAsync(GetCacheFileName(model), data, folder).GetAwaiter().GetResult();
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
                        ExpressionMethod.Eq);
                predicate =
                    predicate == null ? pre
                    : predicate.AndAlso(pre);
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
            _context ??= InitContext();
            var keys = _context.Model.FindEntityType(typeof(TModel)).FindPrimaryKey().Properties
        .Select(x => x.Name);
            foreach (var key in keys)
            {
                result += $"_{GetPropValue(model, key)}";
            }
            return result;
        }

        public virtual async Task AddToCache(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (model != null)
            {
                string key = GetCachedKey(model, _context);
                string folder = $"{CachedFolder}/{key}";
                var view = GetCachedData(model, _context, _transaction);
                await MixCacheService.SetAsync(GetCacheFileName(model), view, folder);
            }
        }

        public virtual async Task RemoveCacheAsync(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (model != null)
            {
                string key = GetCachedKey(model, _context);
                string folder = $"{CachedFolder}/{key}";
                await MixCacheService.RemoveCacheAsync(folder);
            }
        }

        public virtual async Task RemoveCacheAsync(string key, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            string folder = $"{CachedFolder}/{key}";
            await MixCacheService.RemoveCacheAsync(folder);
        }

        private string[] GetKeyMembers(IModel model)
        {
            return model.FindEntityType(typeof(TModel))
                .FindPrimaryKey().Properties.Select(x => x.Name)
                .ToArray();
        }
        private string GetCacheFileName(TModel model)
        {
            return typeof(TView).Name;
        }
        #endregion Cached
    }
}