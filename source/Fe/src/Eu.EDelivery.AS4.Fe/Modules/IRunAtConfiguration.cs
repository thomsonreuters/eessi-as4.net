using Microsoft.Extensions.Configuration;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public interface IRunAtConfiguration : ILifecylceHook
    {
        void Run(IConfigurationBuilder configBuilder);
    }
}