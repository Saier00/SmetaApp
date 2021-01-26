using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SmetaApp.Startup))]
namespace SmetaApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
