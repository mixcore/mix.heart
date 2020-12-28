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
    public class DefaultModelRepository<TDbContext, TModel> :
        Mix.Domain.Data.Repository.ModelRepositoryBase<TDbContext, TModel>
        where TDbContext : DbContext
        where TModel : class
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static volatile DefaultModelRepository<TDbContext, TModel> instance;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private static readonly object syncRoot = new Object();

        /// <summary>
        /// Prevents a default instance of the <see cref="DefaultRepository{TDbContext, TModel, TView}"/> class from being created.
        /// </summary>
        public DefaultModelRepository()
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static DefaultModelRepository<TDbContext, TModel> Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DefaultModelRepository<TDbContext, TModel>();
                    }
                }

                return instance;
            }
        }
    }
}