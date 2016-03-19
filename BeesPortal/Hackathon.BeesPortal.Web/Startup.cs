using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Hackathon.BeesPortal.Web.Startup))]
namespace Hackathon.BeesPortal.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
