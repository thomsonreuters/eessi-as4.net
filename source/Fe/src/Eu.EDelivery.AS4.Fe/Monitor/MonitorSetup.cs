using Eu.EDelivery.AS4.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class MonitorSetup : IMonitorSetup
    {
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<DatastoreContext>(options => { options.UseSqlite(@"FileName=C:\Users\jtilburgh\Documents\My Received Files\messages\messages.db"); });
            services.AddTransient<IMonitorService, MonitorService>();
        }
    }
}