using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Eu.EDelivery.AS4.PayloadService
{
    /// <summary>
    /// Start for the Payload Service Web API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry method for the Payload Service Web API.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                
                // TODO: Hosting URL must be made configurable...
                .UseUrls("http://localhost:3000/")
                .Build();

            host.Run();
        }
    }
}
