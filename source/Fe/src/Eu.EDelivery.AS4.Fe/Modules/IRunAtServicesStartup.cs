using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public interface IRunAtServicesStartup
    {
        void Run(IServiceCollection services);
    }
}