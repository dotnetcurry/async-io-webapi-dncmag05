using System;
using System.Web.Http;

namespace MultipleAsyncInOne {

    public class Global : System.Web.HttpApplication {

        protected void Application_Start(object sender, EventArgs e) {

            GlobalConfiguration.Configuration.Routes.MapHttpRoute("DefaultHttpRoute", "api/{controller}/{action}");
        }
    }
}