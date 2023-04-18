using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QRProject.Startup))]
namespace QRProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
