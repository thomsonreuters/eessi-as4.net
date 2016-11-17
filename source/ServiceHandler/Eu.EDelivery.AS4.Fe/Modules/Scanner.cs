using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Fe.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public class Scanner
    {
        public void Register(IServiceCollection services, Assembly baseAssembly, IList<Assembly> modules, IDictionary<string, string> settings)
        {
            var localModuleImplementations = GetModuleImplementations();
            var moduleTypes = GetModuleTypes(modules);

            var localModules = baseAssembly
                .DefinedTypes
                .Where(x => typeof(IModular).IsAssignableFrom(x.AsType()) && x.IsInterface && x.AsType() != typeof(IModular))
                .Select(x => new
                {
                    Iface = x.AsType(),
                    LocalImplementation = localModuleImplementations.GetImplementation(x.AsType()),
                    FromConfig = settings == null ? string.Empty : settings.Where(cfg => cfg.Key == x.AsType().FullName).Select(cfg => cfg.Value).FirstOrDefault()
                });

            foreach (var local in localModules)
            {
                if (string.IsNullOrEmpty(local.FromConfig) || local.FromConfig == "default")
                {
                    // Check if there is a moduleType
                    var moduleType = moduleTypes.GetImplementation(local.Iface);
                    if (moduleType != null)
                    {
                        // Found a moduleType, register it
                        RegisterType(services, local.Iface, moduleType.AsType());
                        continue;
                    }

                    RegisterType(services, local.Iface, local.LocalImplementation.AsType());
                }
                else
                {
                    var fromConfig = local.FromConfig;
                    var assembly = modules.FirstOrDefault(mod => mod.FullName.Split(',')[0] == fromConfig);
                    if (assembly == null) throw new Exception($"Could not find assembly {fromConfig}, please check the configuration");
                    RegisterType(services, local.Iface, assembly.DefinedTypes);
                }
            }
        }

        private static List<TypeInfo> GetModuleTypes(IEnumerable<Assembly> modules)
        {
            return modules.Select(asm => asm.DefinedTypes.GetImplementation(typeof(IModular))).ToList();
        }

        private IEnumerable<TypeInfo> GetModuleImplementations()
        {
            var localModuleImplementations = Assembly
                .GetEntryAssembly()
                .DefinedTypes
                .Where(x => typeof(IModular).IsAssignableFrom(x.AsType()) && x.IsClass);
            return localModuleImplementations;
        }

        private void RegisterType(IServiceCollection services, Type baseType, Type implementation)
        {
            services.Add(new ServiceDescriptor(baseType, implementation, ServiceLifetime.Singleton));
        }

        private bool RegisterType(IServiceCollection services, Type baseType, IEnumerable<TypeInfo> types)
        {
            var searchFor = types.FirstOrDefault(typ => typ.ImplementedInterfaces.Any(baseType.IsAssignableFrom));
            if (searchFor == null) return false;
            RegisterType(services, baseType, searchFor.AsType());
            return true;
        }
    }
}