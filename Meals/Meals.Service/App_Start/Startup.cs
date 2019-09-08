using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Meals.Service.Startup))]

namespace Meals.Service
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);
        }
    }
}