using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    /// <summary>
    /// Setup at services startup
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.ILifeCycleHook" />
    public interface IRunAtServicesStartup : ILifeCycleHook
    {
        /// <summary>
        /// Run at services setup
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        void Run(IServiceCollection services, IConfigurationRoot configuration);
    }
}