using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(J7Journal.Startup))]

namespace J7Journal
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
            app.MapSignalR();
        }
    }
}