using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GTPool;
using NewsSearch.Infrastructure.Utils;
using NewsSearch.Models;
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

            GTP.Init(2, 25);
        }

        protected void Application_End()
        {
            GlobalCache.StopGarbageCollector();
            GTP.Shutdown();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // To fix an issue with creating a new session for every request
            Session["init"] = 0;

            Utils.Log(string.Format("Session Starting: {0}", new SessionLog(HttpContext.Current)));
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
