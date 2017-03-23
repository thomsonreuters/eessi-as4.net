using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class MonitorSetup : IMonitorSetup
    {
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            var appSettings = configuration.GetSection("Settings").Get<ApplicationSettings>();
            services.AddDbContext<DatastoreContext>(options => { options.UseSqlite($"FileName={appSettings.MessagesDatabase}"); });
            services.AddTransient<IMonitorService, MonitorService>();
        }
    }
}