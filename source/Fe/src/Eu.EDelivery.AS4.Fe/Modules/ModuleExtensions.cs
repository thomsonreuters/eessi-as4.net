using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Eu.EDelivery.AS4.Fe.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public static class ModuleExtensions
    {
        /// <summary>
        ///     Setup module loading
        /// </summary>
        /// <param name="services"></param>
        /// <param name="mappings"></param>
        /// <param name="folderToScan"></param>
        public static void AddModules(this IServiceCollection services, Dictionary<string, string> mappings, string folderToScan = "modules")
        {
            services.AddSingleton<Scanner>();

            var serviceProvider = services.BuildServiceProvider();
            var scanner = serviceProvider.GetService<Scanner>();

            List<TypeInfo> moduleAssemblies = Enumerable.Empty<TypeInfo>().ToList();
            if (Directory.Exists(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, folderToScan)))
            {
                moduleAssemblies = Directory
                    .GetFiles(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, folderToScan), "*.dll")
                    .Select(asm => AssemblyLoadContext.Default.LoadFromAssemblyPath(asm))
                    .SelectMany(asm => asm.DefinedTypes)
                    .ToList();
            }

            var baseTypes = Assembly.GetEntryAssembly().DefinedTypes.ToList();

            scanner
                .Register<IAs4SettingsService>(services, baseTypes, moduleAssemblies, mappings)
                .Register<IRunAtServicesStartup>(services, baseTypes, moduleAssemblies, mappings, ServiceLifetime.Transient)
                .Register<IRunAtAppStartup>(services, baseTypes, moduleAssemblies, mappings, ServiceLifetime.Transient);

            foreach (var runat in services.BuildServiceProvider().GetServices<IRunAtServicesStartup>())
                runat.Run(services);
        }

        public static void ExecuteStartupServices(this IApplicationBuilder builder)
        {
            foreach (var runat in builder.ApplicationServices.GetServices<IRunAtAppStartup>())
                runat.Run(builder);
        }
    }
}