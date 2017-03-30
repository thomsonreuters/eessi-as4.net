using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public interface IRunAtServicesStartup : ILifeCycleHook
    {
        void Run(IServiceCollection services, IConfigurationRoot configuration);
    }
}