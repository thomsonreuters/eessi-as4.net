using Eu.EDelivery.AS4.Fe.Modules;

namespace Eu.EDelivery.AS4.Fe.SmpConfiguration
{
    /// <summary>
    ///     Smp configuration module
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IModular" />
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IRunAtServicesStartup" />
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Modules.IRunAtConfiguration" />
    public interface ISmpConfigurationSetup : IModular, IRunAtServicesStartup, IRunAtConfiguration
    {
    }
}