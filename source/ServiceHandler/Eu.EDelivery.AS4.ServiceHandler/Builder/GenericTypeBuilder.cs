using System;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.ServiceHandler.Builder
{
    /// <summary>
    /// Factory implementation to create instance from a given <see cref="Type"/>
    /// </summary>
    internal class GenericTypeBuilder
    {
        private Type _type;
        private object[] _args;

        /// <summary>
        /// Set the <paramref name="typeString"/>
        /// which has to be created
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public GenericTypeBuilder SetType(string typeString)
        {
            this._type = ResolveType(typeString);
            if (this._type == null) throw new AS4Exception($"Not given class found for given Type: {typeString}");

            return this;
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

        private Type ResolveType(string type)
        {
            return Type.GetType(type, throwOnError: false) ?? Type.GetType(type, name =>
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                return assemblies.FirstOrDefault(z => z.FullName == name.FullName);
            }, typeResolver: null, throwOnError: false);
        }

        /// <summary>
        /// Create an instance of type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T Build<T>() where T : class
        {
            if (this._args != null) return Activator.CreateInstance(this._type, this._args) as T;
            return Activator.CreateInstance(this._type) as T;
        }
    }
}
