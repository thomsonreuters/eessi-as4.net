using System;
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
            const string hostUrl = "http://localhost:3000/";

            IWebHost host =
                new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()

                    // TODO: Hosting URL must be made configurable...
                    .UseUrls(hostUrl)
                    .Build();

            Console.WriteLine("=== Payload Service Started ===");
            host.Run();

            Console.WriteLine("Payload Service shutdown");
        }
    }
}