using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Fe.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public class Scanner
    {
        public void Register(IServiceCollection services, IEnumerable<Assembly> modules)
        {
            var localModuleImplementations = Assembly
                .GetEntryAssembly()
                .DefinedTypes
                .Where(x => typeof(IModular).IsAssignableFrom(x.AsType()) && x.IsClass);

            var localModules = Assembly
                .GetEntryAssembly()
                .DefinedTypes
                .Where(x => typeof(IModular).IsAssignableFrom(x.AsType()) && x.IsInterface && x.AsType() != typeof(IModular))
                .Select(x => new
                {
                    Iface = x.AsType(),
                    LocalImplementation = localModuleImplementations.FirstOrDefault(local => x.IsAssignableFrom(local) && local.IsClass)
                });

            var moduleTypes = modules
                .SelectMany(asm => asm.DefinedTypes.Where(x => typeof(IModular).IsAssignableFrom(x.AsType())))
                .ToList();

            foreach (var local in localModules)
            {
                // Check if there is a moduleType
                var moduleType = moduleTypes.FirstOrDefault(module => module.ImplementedInterfaces.Any(iface => local.Iface.IsAssignableFrom(iface)));
                if (moduleType != null)
                {
                    // Found a moduleType, register it
                    services.Add(new ServiceDescriptor(local.Iface, moduleType.AsType(), ServiceLifetime.Singleton));
                    continue;
                }

                services.Add(new ServiceDescriptor(local.Iface, local.LocalImplementation.AsType(), ServiceLifetime.Singleton));
            }
        }
    }
}