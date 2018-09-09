// Licensed to the Swastika I/O Foundation under one or more agreements.
// The Swastika I/O Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Swastika.Api.Controllers;
using Swastika.UI.Core.SignalR;

namespace Swastika.UI.Core.Controllers
{
    /// <summary>
    /// Api Hub Controller
    /// </summary>
    /// <typeparam name="THub">The type of the hub.</typeparam>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <seealso cref="Swastika.Api.Controllers.BaseApiController{TDbContext, TModel}"/>
    public abstract class ApiHubController<THub, TDbContext, TModel>
        : BaseApiController<TDbContext, TModel>
        where THub : BaseSignalRHub
        where TDbContext : DbContext
        where TModel : class
    {
        /// <summary>
        /// The hub
        /// </summary>
        private readonly IHubContext<THub> _hub;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHubController{THub, TDbContext,
        /// TModel}"/> class.
        /// </summary>
        /// <param name="hub">The hub.</param>
        protected ApiHubController(IHubContext<THub> hub)
        {
            _hub = hub;
            Clients = _hub.Clients;
            Groups = _hub.Groups;
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <value>The clients.</value>
        public IHubClients Clients { get; private set; }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <value>The groups.</value>
        public IGroupManager Groups { get; private set; }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="THub">The type of the hub.</typeparam>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <seealso cref="Swastika.Api.Controllers.BaseApiController{TDbContext, TModel}"/>
    public abstract class ApiHubController<THub, TDbContext, TModel, TView>
        : BaseApiController<TDbContext, TModel, TView>
        where THub : BaseSignalRHub
        where TDbContext : DbContext
        where TModel : class
        where TView : Swastika.Domain.Data.ViewModels.ViewModelBase<TDbContext, TModel, TView>
    {
        /// <summary>
        /// The hub
        /// </summary>
        private readonly IHubContext<THub> _hub;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHubController{THub, TDbContext, TModel,
        /// TView}"/> class.
        /// </summary>
        /// <param name="hub">The hub.</param>
        protected ApiHubController(IHubContext<THub> hub)
        {
            _hub = hub;
            Clients = _hub.Clients;
            Groups = _hub.Groups;
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <value>The clients.</value>
        public IHubClients Clients { get; private set; }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <value>The groups.</value>
        public IGroupManager Groups { get; private set; }
    }
}
