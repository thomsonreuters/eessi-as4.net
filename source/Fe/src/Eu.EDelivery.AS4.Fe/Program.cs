using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Eu.EDelivery.AS4.Fe
{
    public class Program
    {
        public static bool InProcess;
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseEnvironment(InProcess ? "inprocess" : "production")
                .UseKestrel()
                .UseWebRoot("ui/dist")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
