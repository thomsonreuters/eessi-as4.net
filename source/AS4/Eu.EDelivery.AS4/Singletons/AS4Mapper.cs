using System;
using AutoMapper;
using Eu.EDelivery.AS4.Mappings;

namespace Eu.EDelivery.AS4.Singletons
{
    /// <summary>
    /// Singleton to expose Internal Mapper
    /// </summary>
    public sealed class AS4Mapper
    {
        private static readonly Lazy<AS4Mapper> Lazy =
            new Lazy<AS4Mapper>(() => new AS4Mapper());

        public static AS4Mapper Instance => Lazy.Value;

        /// <summary>
        /// Map/Project the given source Model to a given Destination Model
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination Map<TDestination>(object source)
        {
            MapInitialization.InitializeMapper();

            return Mapper.Map<TDestination>(source);
        }
    }
}