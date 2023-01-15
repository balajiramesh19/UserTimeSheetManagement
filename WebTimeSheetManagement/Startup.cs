using Microsoft.Owin;
using Owin;
using System.Net;

[assembly: OwinStartupAttribute(typeof(WebTimeSheetManagement.Startup))]
namespace WebTimeSheetManagement
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
