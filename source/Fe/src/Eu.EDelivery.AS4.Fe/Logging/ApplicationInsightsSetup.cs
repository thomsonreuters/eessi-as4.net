using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Logging
{
    public class ApplicationInsightsSetup : IApplicationInsightsSetup
    {
        private readonly IHostingEnvironment env;

        public ApplicationInsightsSetup(IHostingEnvironment env)
        {
            this.env = env;
        }

        public void Run(IConfigurationBuilder configBuilder)
        {
            if (env.IsEnvironment("Development")) configBuilder.AddApplicationInsightsSettings();
        }

        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);
        }
    }
}