using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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

            Start(isInProcess, CancellationToken.None);
        }

        /// <summary>
        /// Used to start the FrontEnd in process.  Provides cancellation-support so that the FrontEnd
        /// is gracefully closed when the AS4.NET backend stops.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public static void StartInProcess(CancellationToken cancellationToken)
        {
            Start(inProcess: true, cancellationToken: cancellationToken);
        }

        private static void Start(bool inProcess, CancellationToken cancellationToken)
        {
            var config = LoadSettings(inProcess);
            var httpPort = HttpPort(config);

            var host = new WebHostBuilder()
                .UseEnvironment(inProcess ? "inprocess" : "production")
                .UseKestrel()
                .UseWebRoot(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ui/dist"))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(httpPort)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            OpenPortal(inProcess, config);

            host.Run(cancellationToken);
        }

        private static void OpenPortal(bool isInProcess, IConfigurationRoot config)
        {
            if (isInProcess)
                Task.Factory.StartNew(() => Process.Start(config["Port"]));
        }

        private static string HttpPort(IConfigurationRoot config)
        {
            var httpPort = config["Port"] ?? "http://0.0.0.0:5000";
            return httpPort;
        }

        private static IConfigurationRoot LoadSettings(bool isInProcess)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(isInProcess ? "./bin/appsettings.inprocess.json" : "./bin/appsettings.json", true)
                .AddJsonFile(isInProcess ? "appsettings.inprocess.json" : "appsettings.json", true)
                .Build();
            return config;
        }
    }
}