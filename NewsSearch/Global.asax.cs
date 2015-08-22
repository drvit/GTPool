using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GTPool;
using GTP = GTPool.GenericThreadPool;

namespace NewsSearch
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GTP.Init<GtpAsync> (2, 25, 500);
        }

        protected void Application_End()
        {
            GTP.End();
        }

        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    //Log exception
        //    var exception = Server.GetLastError();

        //    Response.Clear();
        //    Server.ClearError();

        //    HttpContext.Current.Response.Redirect("~/");
        //}
    }
}
