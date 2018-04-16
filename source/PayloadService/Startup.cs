using System.IO;
using System.Reflection;
using Eu.EDelivery.AS4.PayloadService.Infrastructure.SwaggerUtils;
using Eu.EDelivery.AS4.PayloadService.Persistance;
using Eu.EDelivery.AS4.PayloadService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace Eu.EDelivery.AS4.PayloadService
{
    /// <summary>
    /// The start point class for the Payload Service Web API.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="env">The hosting environment.</param>
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder =
                new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                                          .AddJsonFile("./bin/appsettings.payloadservice.json", true)
                                          .AddJsonFile($"./appsettings.payloadservice.json", true)
                                          .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        private string AssemblyVersion => GetType().GetTypeInfo().Assembly.GetName().Version.ToString();

        /// <summary>
        /// Gets the <see cref="IConfigurationRoot" /> implementation for the Payload Service Web API.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.s
        /// </summary>
        /// <param name="app">The app.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger Factory.</param>
        /// <param name="appLifetime">The application lifetime.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(
                () => app.ApplicationServices.GetService<CleanUpService>().Start());

            appLifetime.ApplicationStopped.Register(
                () => app.ApplicationServices.GetService<CleanUpService>().Stop());

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseSwagger();

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", $"AS4.NET Payload Service Web API V{AssemblyVersion}"); });

            app.UseMvc();
        }

        /// <summary>
            /// This method gets called by the runtime. Use this method to add services to the container.
            /// </summary>
            /// <param name="services">The services.</param>
            public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPayloadPersister>(
                provider => new FilePayloadPersister(provider.GetService<IHostingEnvironment>()));
        
            // Add framework services.
            services.AddMvc();

            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc(
                        "v1",
                        new Info
                        {
                            Title = "AS4.NET Payload Service",
                            Version = $"v{AssemblyVersion}",
                            Description = "A Web API to upload and download payloads in a persistent manner.",
                            TermsOfService = "None",
                            Contact = new Contact { Name = "DG EMPL" },
                            License =
                                new License
                                {
                                    Name = "EUPL License v1.1.",
                                    Url = "https://joinup.ec.europa.eu/community/eupl/og_page/european-union-public-licence-eupl-v11"
                                }
                        });

                    options.OperationFilter<FileUploadOperation>();
                    options.IncludeXmlComments(GetXmlCommentsPath());
                });
        }

        private static string GetXmlCommentsPath()
        {
            const string xml = "Eu.EDelivery.AS4.PayloadService.xml";
            var app = PlatformServices.Default.Application;
            var binPath = Path.Combine(app.ApplicationBasePath, "bin", xml);
            return File.Exists(binPath) ? binPath : Path.Combine(app.ApplicationBasePath, xml);
        }
    }
}