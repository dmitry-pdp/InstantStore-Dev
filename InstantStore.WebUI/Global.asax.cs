using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

using InstantStore.Domain.Concrete;
using InstantStore.WebUI.Models;

namespace InstantStore.WebUI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = (sender as MvcApplication).Context;
            var httpRequest = httpContext != null ? httpContext.Request : null;
            var exception = this.Server.GetLastError();

            var requestUrl = httpRequest != null ? httpRequest.RawUrl : null;
            var clientIp = httpRequest != null ? httpRequest.ServerVariables["REMOTE_ADDR"] : null;
            var rawUserAgent = httpRequest != null ? httpRequest.ServerVariables["HTTP_USER_AGENT"] : null;
            var rawHeaders = httpRequest != null ? httpRequest.ServerVariables["ALL_RAW"] : null;
            var userId = UserIdentityManager.GetActiveUserId(httpRequest.Cookies);
            new LinqRepository().LogError(exception, DateTime.Now, requestUrl, clientIp, rawUserAgent, rawHeaders, userId);
        }
    }
}