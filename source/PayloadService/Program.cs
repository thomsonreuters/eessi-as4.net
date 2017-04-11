using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Eu.EDelivery.AS4.PayloadService
{
    public class Program
    {
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
