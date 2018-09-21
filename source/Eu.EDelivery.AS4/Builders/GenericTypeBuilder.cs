using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;

namespace Eu.EDelivery.AS4.Builders
{
    /// <summary>
    /// Factory implementation to create instance from a given <see cref="Type"/>
    /// </summary>
    internal class GenericTypeBuilder
    {
        private readonly Type _type;
        private object[] _args;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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
                LogManager.GetCurrentClassLogger().Fatal(message);
                throw new TypeLoadException(message);                
            }

            return FromType(type);
        }

        /// <summary>
        /// Determines whether or not the given <paramref name="typeString"/> can be resolved to a type or not.
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public static bool CanResolveType(string typeString)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(typeString))
                {
                    return false;
                }

                Type type = Type.GetType(typeString, throwOnError: false);
                if (type == null)
                {
                    return false;
                }

                LogPossibleObsolete(typeString, type);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        private static void LogPossibleObsolete(string typeString, Type type)
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
            this._args = args;
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
            if (this._args != null)
            {
                return Activator.CreateInstance(this._type, this._args) as T;
            }
            return Activator.CreateInstance(this._type) as T;
        }
    }
}
