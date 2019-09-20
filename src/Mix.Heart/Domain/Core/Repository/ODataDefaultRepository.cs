// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using System;

namespace Mix.Domain.Data.Repository
{
    /// <summary>
    /// Default Repository with view
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <seealso cref="Mix.Domain.Data.Repository.ModelRepositoryBase{TContext, TModel}" />
    public class ODataDefaultRepository<TDbContext, TModel, TView> :
        Mix.Domain.Data.Repository.ODataViewRepositoryBase<TDbContext, TModel, TView>
        where TDbContext : DbContext
        where TModel : class
        where TView : Mix.Domain.Data.ViewModels.ODataViewModelBase<TDbContext, TModel, TView>
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static volatile ODataDefaultRepository<TDbContext, TModel, TView> instance;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private static readonly object syncRoot = new Object();

        /// <summary>
        /// Prevents a default instance of the <see cref="ODataDefaultRepository{TDbContext, TModel, TView}"/> class from being created.
        /// </summary>
        private ODataDefaultRepository()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ODataDefaultRepository<TDbContext, TModel, TView> Instance {
            get {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ODataDefaultRepository<TDbContext, TModel, TView>();
                    }
                }

                return instance;
            }
        }
    }
}