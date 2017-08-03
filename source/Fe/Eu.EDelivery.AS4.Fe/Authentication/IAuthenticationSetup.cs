using Eu.EDelivery.AS4.Fe.Modules;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    /// <summary>
    /// Setup authentication
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IModular" />
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IRunAtServicesStartup" />
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IRunAtAppConfiguration" />
    public interface IAuthenticationSetup : IModular, IRunAtServicesStartup, IRunAtAppConfiguration
    {
        
    }
}