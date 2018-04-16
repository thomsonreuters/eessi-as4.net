using System;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.PayloadService.Persistance;
using Eu.EDelivery.AS4.PayloadService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            Start(CancellationToken.None);
        }

        /// <summary>
        /// Starts the PayloadService with a cancellation-token
        /// This method is used when the service is started in process.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static void Start(CancellationToken cancellationToken)
        {
            var hostBuilder = new WebHostBuilder();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./bin/appsettings.payloadservice.json", true)
                .AddJsonFile("./appsettings.payloadservice.json", true)
                .AddEnvironmentVariables()
                .Build();

            var url = config.GetValue<string>("Url") ?? "http://localhost:3000";

            var host = hostBuilder
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights();

            int retentionDays = config.GetValue("RetentionPeriod", defaultValue: 90);            
            host.ConfigureServices(services => 
                services.AddSingleton(provider => 
                    new CleanUpService(
                        provider.GetService<IPayloadPersister>(), 
                        TimeSpan.FromDays(retentionDays))));

            

            host.UseUrls(url)
                .Build()
                .RunAsync(cancellationToken)
                .Wait();

            Console.WriteLine("Payload Service shutdown");
        }
    }
}