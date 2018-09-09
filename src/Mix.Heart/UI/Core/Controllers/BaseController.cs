// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Mvc;

namespace Mix.UI.Base.Controllers
{
    /// <summary>
    /// Base Controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class BaseController : Controller
    {
        //TODO: Still need?
        //private readonly IDomainNotificationHandler<DomainNotification> _notifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        public BaseController()
        {
        }

        //TODO: Still need?
        //public BaseController(IDomainNotificationHandler<DomainNotification> notifications) {
        //    _notifications = notifications;
        //}

        //TODO: Still need?
        //public bool IsValidOperation() {
        //    return (!_notifications.HasNotifications());
        //}

        /// <summary>
        /// Creates an <see cref="T:Microsoft.AspNetCore.Mvc.NotFoundResult" /> that produces a <see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound" /> response.
        /// </summary>
        /// <returns>
        /// The created <see cref="T:Microsoft.AspNetCore.Mvc.NotFoundResult" /> for the response.
        /// </returns>
        public override NotFoundResult NotFound()
        {
            return base.NotFound();
        }
    }
}
