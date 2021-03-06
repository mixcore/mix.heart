﻿// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Mix.Infrastructure.Repositories;
using System;

namespace Mix.Heart.Infrastructure.Repositories
{
    /// <summary>
    /// Default Repository with view
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <seealso cref="Mix.Infrastructure.Repositories.ModelRepositoryBase{TContext, TModel}" />
    public class DefaultRepository<TDbContext, TModel, TView> :
        ViewRepositoryBase<TDbContext, TModel, TView>
        where TDbContext : DbContext
        where TModel : class
        where TView : ViewModels.ViewModelBase<TDbContext, TModel, TView>
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static volatile DefaultRepository<TDbContext, TModel, TView> instance;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private static readonly object syncRoot = new Object();

        public DefaultRepository()
        {

        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static DefaultRepository<TDbContext, TModel, TView> Instance {
            get {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DefaultRepository<TDbContext, TModel, TView>();
                    }
                }

                return instance;
            }
        }
    }
}