using Eu.EDelivery.AS4.Fe.Modules;

namespace Eu.EDelivery.AS4.Fe.Swagger
{
    /// <summary>
    /// Setup swagger
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IModular" />
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IRunAtAppConfiguration" />
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IRunAtServicesStartup" />
    public interface ISwaggerSetup : IModular, IRunAtAppConfiguration, IRunAtServicesStartup
    {
        
    }
}