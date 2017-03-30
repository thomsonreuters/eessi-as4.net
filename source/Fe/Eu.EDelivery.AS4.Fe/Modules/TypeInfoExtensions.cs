using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eu.EDelivery.AS4.Fe.Modules
{
    public static class TypeInfoExtensions
    {
        public static TypeInfo GetImplementation(this IEnumerable<TypeInfo> typeInfos, Type baseType)
        {
            return typeInfos.FirstOrDefault(module => module.ImplementedInterfaces.Any(iface => baseType.IsAssignableFrom(baseType)) && module.IsClass);
        }

        public static TypeInfo GetInterface(this IEnumerable<TypeInfo> typeInfos, Type ifaceType)
        {
            return typeInfos.FirstOrDefault(module => module.ImplementedInterfaces.Any(iface => iface == ifaceType) && module.IsInterface);
        }
    }
}