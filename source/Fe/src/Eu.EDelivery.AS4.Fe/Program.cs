using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Eu.EDelivery.AS4.Fe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(args != null && args.Contains("inprocess") ? "appsettings.inprocess.json" : "appsettings.json", optional: true)
                .Build();

            var host = new WebHostBuilder()
                .UseEnvironment(args != null && args.Contains("inprocess") ? "inprocess" : "production")
                .UseKestrel()
                .UseWebRoot("ui/dist")
                .UseUrls(config["Port"] ?? "http://localhost:5000")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
