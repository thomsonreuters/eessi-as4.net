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

            var config = LoadSettings(isInProcess);
            var httpPort = HttpPort(config);

            var host = new WebHostBuilder()
                .UseEnvironment(isInProcess ? "inprocess" : "production")
                .UseKestrel()
                .UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "ui/dist"))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(httpPort)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            OpenPortal(isInProcess, config);

            host.Run();
        }
        private static void OpenPortal(bool isInProcess, IConfigurationRoot config)
        {
            if (isInProcess)
                Task.Factory.StartNew(() => System.Diagnostics.Process.Start(config["Url"]));
        }

        private static string HttpPort(IConfigurationRoot config)
        {
            var httpPort = config["Url"] ?? "http://0.0.0.0:5000";
            return httpPort;
        }

        private static IConfigurationRoot LoadSettings(bool isInProcess)
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(isInProcess ? "appsettings.inprocess.json" : "appsettings.json", optional: true)
                    .Build();
            return config;
        }
    }
}
