using Microsoft.AspNetCore.Builder;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public interface IRunAtAppStartup
    {
        void Run(IApplicationBuilder app);
    }
}