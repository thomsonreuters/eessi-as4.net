using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.Database;
using Eu.EDelivery.AS4.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Setup monitor
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Monitor.IMonitorSetup" />
    public class MonitorSetup : IMonitorSetup
    {
        private static readonly string Section = "Monitor";

        /// <summary>
        /// Runs the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            var settings = configuration.GetSection(Section).Get<MonitorSettings>();

            services.AddDbContext<DatastoreContext>(options => SqlConnectionBuilder.Build(settings.Provider, settings.ConnectionString, options));
            services.AddScoped<IMonitorService, MonitorService>();
            services.AddScoped<IDatastoreRepository, DatastoreRepository>();
            services.AddSingleton<IClient, Client>();
        }

        /// <summary>
        /// Runs the specified configuration builder.
        /// </summary>
        /// <param name="configBuilder">The configuration builder.</param>
        /// <param name="services">The services.</param>
        /// <param name="localConfig">The local configuration.</param>
        public void Run(IConfigurationBuilder configBuilder, IServiceCollection services, IConfigurationRoot localConfig)
        {
            services.Configure<MonitorSettings>(localConfig.GetSection(Section));
        }
    }
}