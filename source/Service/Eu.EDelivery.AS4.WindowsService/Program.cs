using System.ServiceProcess;

namespace Eu.EDelivery.AS4.WindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new AS4Service()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
