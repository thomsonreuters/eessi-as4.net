using System;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Builders
{
    /// <summary>
    /// Factory implementation to create instance from a given <see cref="Type"/>
    /// </summary>
    public class GenericTypeBuilder
    {
        private readonly Type _type;
        private object[] _args;

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
                throw new AS4Exception($"Not given class found for given Type: {typeString}");
            }

            return FromType(type);
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
            if (this._args != null) return Activator.CreateInstance(this._type, this._args) as T;
            return Activator.CreateInstance(this._type) as T;
        }
    }
}
