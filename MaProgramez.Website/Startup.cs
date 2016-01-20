using System.Threading.Tasks;
using MaProgramez.Website.Membership;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MaProgramez.Website.Startup))]

namespace MaProgramez.Website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var idProvider = new CustomUserIdProvider();
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);

            // Automatic processing service run on a separate thread
            var automaticProcessingThread = Task.Run(() =>
            {
                var automaticProcesingServicePeriodic = new AutomaticProcessingService.AutomaticProcessingService();
            });

            app.MapSignalR();
        }
    }
}