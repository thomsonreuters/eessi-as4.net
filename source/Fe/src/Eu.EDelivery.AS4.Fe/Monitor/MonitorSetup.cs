using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class MonitorSetup : IMonitorSetup
    {
        private static readonly string Section = "Monitor";

        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            var settings = configuration.GetSection(Section).Get<MonitorSettings>();

            services.AddDbContext<DatastoreContext>(options => SqlConnectionBuilder.Build(settings.Provider, settings.ConnectionString, options));
            services.AddTransient<IMonitorService, MonitorService>();
        }

        public void Run(IConfigurationBuilder configBuilder, IServiceCollection services, IConfigurationRoot localConfig)
        {
            services.Configure<MonitorSettings>(localConfig.GetSection(Section));
        }
    }
}