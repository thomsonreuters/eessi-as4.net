using Microsoft.AspNetCore.Builder;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public interface IRunAtAppStartup : ILifeCycleHook
    {
        void Run(IApplicationBuilder app);
    }
}