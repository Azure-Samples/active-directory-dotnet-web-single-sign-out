using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebAppDistributedSignOutDotNet.App_Start.OwinStartup))]

namespace WebAppDistributedSignOutDotNet.App_Start
{
    public partial class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }

    }
}
