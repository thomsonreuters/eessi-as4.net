using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Eu.EDelivery.AS4.Fe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var isInProcess = args != null && args.Contains("inprocess");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(isInProcess ? "appsettings.inprocess.json" : "appsettings.json", optional: true)
                .Build();

            var httpPort = config["Port"] ?? "http://0.0.0.0:5000";

            var host = new WebHostBuilder()
                .UseEnvironment(isInProcess ? "inprocess" : "production")
                .UseKestrel()
                .UseWebRoot("ui/dist")
                .UseUrls(httpPort)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            if (isInProcess && !string.IsNullOrEmpty(config["StartUrl"]))
                Task.Factory.StartNew(() => System.Diagnostics.Process.Start(config["StartUrl"]));

            host.Run();
        }
    }
}
