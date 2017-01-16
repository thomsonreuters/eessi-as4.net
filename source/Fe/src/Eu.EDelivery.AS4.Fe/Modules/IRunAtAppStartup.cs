using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public interface IRunAtAppStartup
    {
        void Run(IApplicationBuilder app);
    }

    public interface IRunAtConfiguration
    {
        void Run(IConfigurationBuilder configBuilder);
    }
}