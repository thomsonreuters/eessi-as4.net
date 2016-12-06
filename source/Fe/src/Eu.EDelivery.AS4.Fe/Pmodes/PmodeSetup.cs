using Eu.EDelivery.AS4.Fe.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    public class PmodeSetup : IRunAtServicesStartup
    {
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<PmodeSettings>(configuration.GetSection("Pmodes"));

            services.AddSingleton<IAs4PmodeSource, As4PmodeSource>();
            services.AddSingleton<IAs4PmodeService, As4PmodeService>();
        }
    }
}