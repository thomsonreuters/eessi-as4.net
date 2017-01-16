using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Start;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Automapper
{
    public class AutomapperSetup : IRunAtServicesStartup
    {
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddAutoMapper();
        }
    }
}