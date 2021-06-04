// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mix.Common.Helper;
using Mix.Heart.Constants;
using Mix.Heart.Infrastructure.Interfaces;
using Mix.Heart.Infrastructure.Repositories;
using Mix.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
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
        #region Properties

        protected IMediator _mediator;

        public void SetMediator(IMediator mediator)
        {
            this._mediator = mediator;
        }

        [JsonIgnore]
        public bool IsCache { get; set; } = MixCommonHelper.GetWebConfig<bool>(WebConfiguration.IsCache);

        [JsonIgnore]
        private string ModelName { get { return typeof(TModel).FullName; } }

        [JsonIgnore]
        public string CachedFolder { get { return $"{ModelName}/"; } }

        [JsonIgnore]
        private string CachedFileName { get { return typeof(TView).Name; } }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        private bool isValid = true;

        /// <summary>
        /// The mapper
        /// </summary>
        private IMapper _mapper;

        /// <summary>
        /// The model
        /// </summary>
        private TModel _model;

        /// <summary>
        /// The model mapper
        /// </summary>
        private IMapper _modelMapper;

        [JsonIgnore]
        public static DefaultRepository<TDbContext, TModel, TView> Repository;

        [JsonIgnore]
        public static DefaultModelRepository<TDbContext, TModel> ModelRepository;

        static ViewModelBase()
        {
            Repository = new DefaultRepository<TDbContext, TModel, TView>();
            ModelRepository = new DefaultModelRepository<TDbContext, TModel>();
        }

        /// <summary>
        /// Gets or sets the mapper.
        /// </summary>
        /// <value>
        /// The mapper.
        /// </value>
        [JsonIgnore]
        private IMapper Mapper
        {
            get { return _mapper ?? (_mapper = this.CreateMapper()); }
            set => _mapper = value;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [JsonIgnore]
        public TModel Model
        {
            get
            {
                if (_model == null)
                {
                    Type classType = typeof(TModel);
                    ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
                    _model = (TModel)classConstructor.Invoke(new object[] { });
                }
                return _model;
            }
            set => _model = value;
        }

        /// <summary>
        /// Gets or sets the model mapper.
        /// </summary>
        /// <value>
        /// The model mapper.
        /// </value>
        [JsonIgnore]
        private IMapper ModelMapper
        {
            get { return _modelMapper ?? (_modelMapper = this.CreateModelMapper()); }
            set => _modelMapper = value;
        }

        /// <summary>
        /// Creates the mapper.
        /// </summary>
        /// <returns></returns>
        private IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TModel, TView>().ReverseMap());
            var mapper = new Mapper(config);
            return mapper;
        }

        /// <summary>
        /// Creates the model mapper.
        /// </summary>
        /// <returns></returns>
        private IMapper CreateModelMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TModel, TModel>().ReverseMap());
            var mapper = new Mapper(config);
            return mapper;
        }

        [JsonIgnore]
        public List<string> Errors { get; set; } = new List<string>();

        [JsonIgnore]
        [JsonProperty("isValid")]
        public bool IsValid { get => isValid; set => isValid = value; }

        #endregion Properties

        #region Common

        /// <summary>
        /// Initializes the model.
        /// </summary>
        /// <returns></returns>
        public virtual TModel InitModel()
        {
            Type classType = typeof(TModel);
            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
            TModel context = (TModel)classConstructor.Invoke(new object[] { });

            return context;
        }

        /// <summary>
        /// Parses the model.
        /// </summary>
        /// <returns></returns>
        public virtual TModel ParseModel(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            this.Model = InitModel();
            Mapper.Map((TView)this, Model);
            return this.Model;
        }

        /// <summary>
        /// Initializes the view.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="isLazyLoad">if set to <c>true</c> [is lazy load].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual TView InitView(TModel model = null, bool isLazyLoad = true, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            Type classType = typeof(TView);

            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
            if (model == null && classConstructor != null)
            {
                return (TView)classConstructor.Invoke(new object[] { });
            }
            else
            {
                classConstructor = classType.GetConstructor(new Type[] { typeof(TModel), typeof(bool), typeof(TDbContext), typeof(IDbContextTransaction) });
                if (classConstructor != null)
                {
                    return (TView)classConstructor.Invoke(new object[] { model, isLazyLoad, _context, _transaction });
                }
                else
                {
                    classConstructor = classType.GetConstructor(new Type[] { typeof(TModel), typeof(TDbContext), typeof(IDbContextTransaction) });
                    return (TView)classConstructor.Invoke(new object[] { model, _context, _transaction });
                }
            }
        }

        /// <summary>
        /// Parses the view.
        /// </summary>
        /// <param name="isExpand">if set to <c>true</c> [is expand].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual TView ParseView(bool isExpand = true, TDbContext _context = null, IDbContextTransaction _transaction = null
                                                    )
        {
            //AutoMapper.Mapper.Map<TModel, TView>(Model, (TView)this);
            Mapper.Map<TModel, TView>(Model, (TView)this);
            if (isExpand)
            {
                UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
                try
                {
                    ExpandView(context, transaction);
                }
                catch (Exception ex)
                {
                    Repository.LogErrorMessage(ex);
                    if (isRoot)
                    {
                        //if current transaction is root transaction
                        transaction.Rollback();
                    }
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
            return (TView)this;
        }

        /// <summary>
        /// Expands the view.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        public virtual void ExpandView(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
        }
        /// <summary>
        /// Validates the specified context.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        public virtual void Validate(TDbContext _context, IDbContextTransaction _transaction)
        {
            var validateContext = new System.ComponentModel.DataAnnotations.ValidationContext(this, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            IsValid = Validator.TryValidateObject(this, validateContext, results);
            if (!IsValid)
            {
                Errors.AddRange(results.Select(e => e.ErrorMessage));
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
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        #endregion Common

        #region Contructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{TDbContext, TModel, TView}"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        protected ViewModelBase(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            this.Model = model;
            ParseView(_context: _context, _transaction: _transaction);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{TDbContext, TModel, TView}"/> class.
        /// </summary>
        protected ViewModelBase()
        {
            //this.Model = InitModel();
            //ParseView(isExpand: false);
        }

        protected ViewModelBase(SerializationInfo info, StreamingContext context)
        {
        }

        #endregion Contructor

        #region Cached

        public string GetCachedKey(TModel model, TDbContext _context, IDbContextTransaction _transaction)
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

        public object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName)?.GetValue(src, null);
        }

        public virtual Task GenerateCache(TModel model, TView view, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            Task result = null;
            try
            {
                var removeTask = Task.Factory.StartNew(() =>
                {
                    RemoveCache(model);
                });

                var tasks = new List<Task>();
                tasks.Add(AddToCache(model, view, context, transaction));
                //tasks.AddRange(GenerateRelatedData(context, transaction));
                var viewTypes = Assembly.GetAssembly(typeof(TView)).GetTypes()
                                      .Where(t => t.Name != typeof(TView).Name && t.Namespace == typeof(TView).Namespace && t.MemberType == MemberTypes.TypeInfo)
                                      .ToList();
                foreach (var classType in viewTypes)
                {
                    ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { model.GetType(), typeof(TDbContext), typeof(IDbContextTransaction) });
                    MethodInfo magicMethod = classType.GetMethod("AddToCache", new Type[] { model.GetType(), typeof(object), typeof(TDbContext), typeof(IDbContextTransaction) });
                    if (classConstructor != null && magicMethod != null)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            var otherview = classConstructor.Invoke(new object[] { model, context, transaction });
                            magicMethod.Invoke(otherview, new object[] { model, otherview, context, transaction });
                        }));
                    }
                }
                result = Task.WhenAll(tasks);
                return removeTask.ContinueWith(resp =>
                {
                    result.Wait();
                });
            }
            catch (Exception ex)
            {
                UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
                return Task.CompletedTask;
            }
            finally
            {
                if (isRoot && (result.Status == TaskStatus.RanToCompletion || result.Status == TaskStatus.Canceled || result.Status == TaskStatus.Faulted))
                {
                    //if current Context is Root
                    UnitOfWorkHelper<TDbContext>.CloseDbContext(ref context, ref transaction);
                }
            }
        }

        //public virtual List<Task> GenerateRelatedData(TDbContext context, IDbContextTransaction transaction)
        //{
        //    var tasks = new List<Task>();
        //    return tasks;
        //}

        public virtual Task AddToCache(TModel model, object data, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            if (data == null)
            {
                Model = model;
                data = ParseView(true, _context, _transaction);
            }
            string key = GetCachedKey(model, _context, _transaction);
            string folder = $"{CachedFolder}/{key}";
            MixCacheService.Set(CachedFileName, data, folder);
            return Task.CompletedTask;
        }

        public virtual Task RemoveCache(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            string key = GetCachedKey(model, _context, _transaction);
            string folder = $"{CachedFolder}/{key}";
            MixCacheService.RemoveCacheAsync(folder);
            return Task.CompletedTask;
        }
        #endregion Cached
    }
}