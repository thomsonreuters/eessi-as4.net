using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public interface IRunAtServicesStartup : ILifecylceHook
    {
        void Run(IServiceCollection services, IConfigurationRoot configuration);
    }
}