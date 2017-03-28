using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public interface IRunAtConfiguration : ILifeCycleHook
    {
        void Run(IConfigurationBuilder configBuilder, IServiceCollection services, IConfigurationRoot localConfig);
    }
}