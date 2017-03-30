using Eu.EDelivery.AS4.Fe.Modules;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    public interface IAuthenticationSetup : IModular, IRunAtServicesStartup, IRunAtAppStartup
    {
        
    }
}