using InstantStore.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InstantStore.WebUI.Models
{
    /// <summary>
    /// Custom action / controller authorization attribute.
    /// </summary>
    /// <remarks>
    /// This will be removed when migrated to asp.net identity.
    /// </remarks>
    public class CustomAuthorizationAttribute : AuthorizeAttribute
    {
        private bool isAdmin;

        public CustomAuthorizationAttribute(bool isAdmin)
        {
            this.isAdmin = isAdmin;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = UserIdentityManager.GetActiveUser(httpContext.Request, new LinqRepository());
            bool authorized = user != null && !user.IsBlocked && (!isAdmin || user.IsAdmin);
            if (authorized)
            {
                httpContext.AssignUser(user);
            }
            return authorized;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}