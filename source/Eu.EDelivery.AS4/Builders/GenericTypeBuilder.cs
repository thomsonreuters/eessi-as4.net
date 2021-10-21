using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace Eu.EDelivery.AS4.Builders
{
    /// <summary>
    /// Factory implementation to create instance from a given <see cref="Type"/>
    /// </summary>
    internal class GenericTypeBuilder
    {
        private readonly Type _type;
        private object[] _args;

        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

        private GenericTypeBuilder(Type type)
        {
            _type = type;
        }

        /// <summary>
        /// Initializes a new GenericTypeBuilder to instantiate a type for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GenericTypeBuilder FromType(Type type)
        {
            return new GenericTypeBuilder(type);
        }

        /// <summary>
        /// Initializes a new GenericTypeBuilder to instantiate a type for the specified typeString.
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public static GenericTypeBuilder FromType(string typeString)
        {
            var type = ResolveType(typeString);

            if (type == null)
            {
                string message = $"Type not found: {typeString}";
                Logger.Fatal(message);
                throw new TypeLoadException(message);                
            }

            return FromType(type);
        }

        /// <summary>
        /// Determines whether or not the given <paramref name="typeString"/> can be resolved to a specified generic type or not.
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public static bool CanResolveTypeThatImplements<T>(string typeString)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(typeString))
                {
                    Logger.Error($"Cannot resolve type string: {typeString} to a {typeof(T).Name} instance because the type string is blank");
                    return false;
                }

                Type type = Type.GetType(typeString, throwOnError: false);
                if (type == null)
                {
                    Logger.Error($"Cannot resolve type string: {typeString} to a {typeof(T).Name} instance because the type is not found in this AppDomain");
                    return false;
                }

                LogPossibleObsoleteMessage(typeString, type);

                if (type.GetInterfaces().All(i => i != typeof(T)))
                {
                    Logger.Error($"Cannot resolve type string: {typeString} to a {typeof(T).Name} instance because the type does not implement ");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Cannot resolve type string: {typeString}: " + ex.Message);
                Logger.Error(ex);
                return false;
            }
        }

        private static void LogPossibleObsoleteMessage(string typeString, Type type)
        {
            IEnumerable<ObsoleteAttribute> obsoleteAttrs =
                type.GetCustomAttributes(typeof(ObsoleteAttribute))
                    .OfType<ObsoleteAttribute>();

            foreach (ObsoleteAttribute oa in obsoleteAttrs)
            {
                Logger.Warn($"Type: {typeString} is obsolete: {oa.Message}");
            }
        }

        /// <summary>
        /// Set the <paramref name="args"/>
        /// which has to be send as arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public GenericTypeBuilder SetArgs(params object[] args)
        {
            _args = args;
            return this;
        }
       
        private static Type ResolveType(string type)
        {
            return Type.GetType(type, throwOnError: false) ?? Type.GetType(type, name =>
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                return assemblies.FirstOrDefault(a => a.FullName == name.FullName);
            }, typeResolver: null, throwOnError: false);
        }

        /// <summary>
        /// Create an instance of type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Build<T>() where T : class
        {
            T t = _args == null
                ? Activator.CreateInstance(_type) as T
                : Activator.CreateInstance(_type, _args) as T;

            if (t == null)
            {
                throw new InvalidOperationException(
                    $"Unable to create {_type} as an instance of {typeof(T).Name}");
            }

            return t;
        }
    }
}
