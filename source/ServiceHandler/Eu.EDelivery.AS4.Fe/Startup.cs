using System.Linq;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Start;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using NLog.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Eu.EDelivery.AS4.Fe.Settings;

namespace Eu.EDelivery.AS4.Fe
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

            //if (env.IsEnvironment("Development"))
            //    builder.AddApplicationInsightsSettings(true);

            //builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();
            services.AddSwaggerGen();
            services.AddAutoMapper();
            services.AddSingleton<Scanner>();
            services.AddSingleton<ILogging, Logging.Logging>();
            services.AddOptions();

            services.Configure<ApplicationSettings>(Configuration.GetSection("Settings"));

            // Setup modular implementations
            var serviceProvider = services.BuildServiceProvider();
            var scanner = serviceProvider.GetService<Scanner>();

            var moduleAssemblies = Directory
                .GetFiles(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "modules"), "*.dll")
                .Select(asm => AssemblyLoadContext.Default.LoadFromAssemblyPath(asm));

            scanner.Register(services, Assembly.GetEntryAssembly(), moduleAssemblies.ToList(), serviceProvider.GetService<IOptions<ApplicationSettings>>().Value.Modules);            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddNLog();

            env.ConfigureNLog("nlog.config");

            var logger = app.ApplicationServices.GetService<ILogging>();
            var settings = app.ApplicationServices.GetService<IOptions<ApplicationSettings>>();
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        var response = new
                        {
                            IsError = true,
                            Exception = !settings.Value.ShowStackTraceInExceptions ? null : ex.Error.StackTrace
                        };
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                        logger.Error(ex.Error);
                    }
                });
            });

            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}