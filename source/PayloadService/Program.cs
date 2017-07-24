using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

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
            var hostBuilder = new WebHostBuilder();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{hostBuilder.GetSetting("Environment")}.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var url = config.GetValue<string>("Url") ?? "http://localhost:5000";

            var host = hostBuilder
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights();


            Console.WriteLine("=== Payload Service Started ===");
            host
                .UseUrls(host.GetSetting(url))
                .Build()
                .Run();

            Console.WriteLine("Payload Service shutdown");
        }
    }
}