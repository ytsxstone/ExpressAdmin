using System;
using System.Collections;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;

using ExpressWeb.Authorizes;

namespace ExpressWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Application
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalFilters.Filters.Add(new CustomHandleErrorAttribute());
            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// Session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Session_End(object sender, EventArgs e)
        {
            Hashtable hOnline = (Hashtable)Application["Online"];
            if (null != hOnline && hOnline[Session.SessionID] != null)
            {
                hOnline.Remove(Session.SessionID);
                Application.Lock();
                Application["Online"] = hOnline;
                Application.UnLock();
            }
        }
    }
}
