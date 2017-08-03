using System;
using System.Reflection;
using Microsoft.AspNet.SignalR.Infrastructure;
using Newtonsoft.Json.Serialization;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Contract resolver to support camel casing in SignalR
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.IContractResolver" />
    public class SignalRContractResolver : IContractResolver
    {
        private readonly Assembly assembly;
        private readonly IContractResolver camelCaseContractResolver;
        private readonly IContractResolver defaultContractSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRContractResolver"/> class.
        /// </summary>
        public SignalRContractResolver()
        {
            defaultContractSerializer = new DefaultContractResolver();
            camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
            assembly = typeof(Connection).GetTypeInfo().Assembly;
        }


        /// <summary>
        /// Resolves the contract for a given type.
        /// </summary>
        /// <param name="type">The type to resolve a contract for.</param>
        /// <returns>
        /// The contract for a given type.
        /// </returns>
        public JsonContract ResolveContract(Type type)
        {
            return type.GetTypeInfo().Assembly.Equals(assembly) ? defaultContractSerializer.ResolveContract(type) : camelCaseContractResolver.ResolveContract(type);
        }

    }
}