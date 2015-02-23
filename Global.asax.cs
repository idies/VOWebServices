using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Compilation;
namespace VOServices
{
    /// <summary>
    /// Routing implemented.
    /// @Deoyani Nandrekar-Heinis Feb 2015
    /// </summary>
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes(RouteTable.Routes);
        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.Add(new System.Web.Routing.Route("{anything1}/SDSSConeSearch.asmx/{*pathinfo}", new WebServiceRouteHandler("~/SDSSConeSearch.asmx")));            
            routes.Add(new System.Web.Routing.Route("{anything1}/sdssFields.asmx/{*pathinfo}", new WebServiceRouteHandler("~/sdssFields.asmx")));
            routes.Add(new System.Web.Routing.Route("{anything1}/SIAP.asmx/{*pathinfo}", new WebServiceRouteHandler("~/SIAP.asmx")));
            
        }

        protected void Session_Start(object sender, EventArgs e)
        {
           
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
           
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }

    public class WebServiceRouteHandler : IRouteHandler
    {
        private string virtualPath;

        public WebServiceRouteHandler(string virtualPath)
        {
            this.virtualPath = virtualPath;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new System.Web.Services.Protocols.WebServiceHandlerFactory().GetHandler(HttpContext.Current, "*", this.virtualPath, HttpContext.Current.Server.MapPath(this.virtualPath));
        }        
    }   
}