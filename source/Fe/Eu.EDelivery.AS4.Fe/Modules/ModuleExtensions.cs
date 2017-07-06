using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        /// <param name="configBuilder"></param>
        /// <param name="configuration"></param>
        /// <param name="folderToScan"></param>
        public static void AddModules(this IServiceCollection services, Dictionary<string, string> mappings, Action<IConfigurationBuilder, IHostingEnvironment> configBuilder, out IConfigurationRoot configuration, string folderToScan = "modules")
        {
            services.AddSingleton<Scanner>();

            List<TypeInfo> moduleAssemblies;
            var scanner = SetupAssemblyScanner(services, folderToScan, out moduleAssemblies);

            var baseTypes = typeof(Startup).Assembly.DefinedTypes.ToList();
            RegisterInterfaces(services, mappings, scanner, baseTypes, moduleAssemblies);

            var configurationBuilder = new ConfigurationBuilder();
            configBuilder(configurationBuilder, services.BuildServiceProvider().GetService<IHostingEnvironment>());
            var localConfig = configurationBuilder.Build();
            configuration = localConfig;
            CallStartup<IRunAtConfiguration>(services, service => service.Run(configurationBuilder, services, localConfig));

            CallStartup<IRunAtServicesStartup>(services, service => service.Run(services, localConfig));
        }

        public static void ExecuteStartupServices(this IApplicationBuilder builder)
        {
            foreach (var runat in builder.ApplicationServices.GetServices<IRunAtAppConfiguration>())
                runat.Run(builder);
        }

        private static Scanner SetupAssemblyScanner(IServiceCollection services, string folderToScan, out List<TypeInfo> moduleAssemblies)
        {
            var serviceProvider = services.BuildServiceProvider();
            var scanner = serviceProvider.GetService<Scanner>();

            moduleAssemblies = Enumerable.Empty<TypeInfo>().ToList();
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
            return scanner;
        }

        private static void RegisterInterfaces(IServiceCollection services, Dictionary<string, string> mappings, Scanner scanner, List<TypeInfo> baseTypes, List<TypeInfo> moduleAssemblies)
        {
            // Get IModular interface
            foreach (var iface in baseTypes.Where(type => typeof(IModular).IsAssignableFrom(type) && type.IsInterface && type != typeof(IModular)))
            {
                scanner.Register(iface, services, baseTypes, moduleAssemblies, mappings);
            }
        }

        private static void CallStartup<T>(IServiceCollection services, Action<T> caller)
        {
            foreach (var runat in services.BuildServiceProvider().GetServices<T>()) caller(runat);
        }
    }
}