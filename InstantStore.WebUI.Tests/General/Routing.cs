using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Routing;
using System.Web.Mvc;
using Moq;
using System.Web;


namespace InstantStore.WebUI.Tests.General
{
    [TestClass]
    public class Routing
    {
        /*Reference: http://bradwilson.typepad.com/blog/2010/07/testing-routing-and-url-generation-in-aspnet-mvc.html */

        /*Incoming*/
        [TestMethod]
        public void Can_Route_Empty_Url()
        {
            // Arrange
            var context = new StubHttpContextForRouting(requestUrl: "~/");
            var routes = new RouteCollection();
            RouteConfig.RegisterRoutes(routes);

            // Act
            RouteData routeData = routes.GetRouteData(context);

            // Assert
            Assert.IsNotNull(routeData, "Route data is null");
            Assert.AreEqual("Product", routeData.Values["controller"], "Problem with controller parameter");
            Assert.AreEqual("List", routeData.Values["action"], "Problem with action parameter");
            Assert.AreEqual(null, routeData.Values["category"], "Problem with category parameter");
            Assert.AreEqual(1, routeData.Values["page"], "Problem with page parameter");
        }

        /*Outgoing*/
        [TestMethod]
        public void ActionWithSpecificControllerAndAction()
        {
            UrlHelper helper = GetUrlHelper();

            string url = helper.Action("action", "controller");

            Assert.AreEqual("/controller/action", url);
        }

        static UrlHelper GetUrlHelper(string appPath = "/", RouteCollection routes = null)
        {
            if (routes == null)
            {
                routes = new RouteCollection();
                RouteConfig.RegisterRoutes(routes);
            }

            HttpContextBase httpContext = new StubHttpContextForRouting(appPath);
            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "defaultcontroller");
            routeData.Values.Add("action", "defaultaction");
            RequestContext requestContext = new RequestContext(httpContext, routeData);
            UrlHelper helper = new UrlHelper(requestContext, routes);
            return helper;
        }

    }
}
