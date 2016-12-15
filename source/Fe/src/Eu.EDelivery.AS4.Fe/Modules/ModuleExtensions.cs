using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Fe.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
#if coreclr
using System.Runtime.Loader;
#endif

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
        public static void AddModules(this IServiceCollection services, Dictionary<string, string> mappings, IConfigurationRoot configuration, string folderToScan = "modules")
        {
            services.AddSingleton<Scanner>();

            var serviceProvider = services.BuildServiceProvider();
            var scanner = serviceProvider.GetService<Scanner>();

            var moduleAssemblies = Enumerable.Empty<TypeInfo>().ToList();
            if (Directory.Exists(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, folderToScan)))
                moduleAssemblies = Directory
                    .GetFiles(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, folderToScan), "*.dll")
#if coreclr
                    .Select(asm => AssemblyLoadContext.Default.LoadFromAssemblyPath(asm))
#else
                    .Select(Assembly.LoadFile)
#endif
                    .SelectMany(asm => asm.DefinedTypes)
                    .ToList();

            var baseTypes = Assembly.GetEntryAssembly().DefinedTypes.ToList();

            scanner
                .Register<IAs4SettingsService>(services, baseTypes, moduleAssemblies, mappings)
                .Register<IRunAtServicesStartup>(services, baseTypes, moduleAssemblies, mappings, ServiceLifetime.Transient)
                .Register<IRunAtAppStartup>(services, baseTypes, moduleAssemblies, mappings, ServiceLifetime.Transient);

            foreach (var runat in services.BuildServiceProvider().GetServices<IRunAtServicesStartup>())
                runat.Run(services, configuration);
        }

        public static void ExecuteStartupServices(this IApplicationBuilder builder)
        {
            foreach (var runat in builder.ApplicationServices.GetServices<IRunAtAppStartup>())
                runat.Run(builder);
        }
    }
}