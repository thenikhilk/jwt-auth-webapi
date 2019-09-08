namespace Meals.Service
{
    using Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System.Data.Entity;
    using System.Web.Http;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            //Uncomment to restrict access to all endpoints(except the OAuth endpoint) to requests that have been authenticated(a request that sends along a valid Jwt)
            //GlobalConfiguration.Configure(FilterConfig.Configure);
            Database.SetInitializer(new Initializer());
            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
