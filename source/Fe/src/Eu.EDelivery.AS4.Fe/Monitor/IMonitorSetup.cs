using Eu.EDelivery.AS4.Fe.Modules;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public interface IMonitorSetup : IModular, IRunAtServicesStartup, IRunAtConfiguration
    {
    }
}