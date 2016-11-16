using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Eu.EDelivery.AS4.Fe.Services;
using Eu.EDelivery.AS4.Fe.Start;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.PlatformAbstractions;

namespace Eu.EDelivery.AS4.Fe
{
    public class Scanner
    {
        private readonly Func<AssemblyName, Assembly> assemblyLoader = (name) => Assembly.Load(name);

        public void RegisterAssembly(IServiceCollection services, AssemblyName assemblyName)
        {
            var assembly = assemblyLoader(assemblyName);
            foreach (var type in assembly.DefinedTypes)
            {
                //var dependencyAttributes = type.GetCustomAttributes<DependencyAttribute>();
                //// Each dependency can be registered as various types
                //foreach (var dependencyAttribute in dependencyAttributes)
                //{
                //    var serviceDescriptor = dependencyAttribute.BuildServiceDescriptor(type);
                //    services.Add(serviceDescriptor);
                //}
            }
        }

        public void Register(IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                
            }
        }
    }

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);
                //.AddJsonFile("appsettings.json", true, true)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

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
            services.AddTransient<IAs4SettingsService, As4SettingsService>();
            services.AddSwaggerGen();
            services.AddAutoMapper();
            services.AddSingleton<Scanner>();

            var scanner = services
                .BuildServiceProvider()
                .GetService<Scanner>();
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            foreach (string dll in Directory.GetFiles(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "modules"), "*.dll"))
            {
                scanner.Register(services, AssemblyLoadContext.Default.LoadFromAssemblyPath(dll));
            }
                
            
            //.RegisterAssembly(services, );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}