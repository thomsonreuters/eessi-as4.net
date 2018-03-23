using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.SmpConfiguration
{
    /// <summary>
    ///     Implementation of <see cref="ISmpConfigurationSetup" />
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SmpConfiguration.ISmpConfigurationSetup" />
    public class SmpConfigurationSetup : ISmpConfigurationSetup
    {
        /// <summary>
        ///     Runs the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped<ISmpConfigurationService, SmpConfigurationService>();
        }

        /// <summary>
        ///     Runs the specified configuration builder.
        /// </summary>
        /// <param name="configBuilder">The configuration builder.</param>
        /// <param name="services">The services.</param>
        /// <param name="localConfig">The local configuration.</param>
        public void Run(IConfigurationBuilder configBuilder, IServiceCollection services,
            IConfigurationRoot localConfig)
        {
        }
    }
}