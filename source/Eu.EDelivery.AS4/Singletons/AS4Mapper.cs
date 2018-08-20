using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NLog;

namespace Eu.EDelivery.AS4.Singletons
{
    /// <summary>
    /// Singleton to expose Internal Mapper
    /// </summary>
    public static class AS4Mapper
    {
        private static readonly object Locker = new object();
        private static bool _isInitialized;

        /// <summary>
        /// Gets a collection of the mapping profiles defined in AS4.
        /// Integrators can call this method and use the returned profiles to add to their own mapping configuration.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetAS4MappingProfiles()
        {
            return typeof(AS4Mapper).Assembly.ExportedTypes.Where(typeof(Profile).IsAssignableFrom);
        }

        /// <summary>
        /// Performs the initialization process of all the mappings so the AS4 component can use it using the <see cref="Map{TDestination}"/> method.
        /// </summary>
        public static void Initialize()
        {
            lock (Locker)
            {
                if (_isInitialized) { return; }
                _isInitialized = true;
            }

            LogManager.GetCurrentClassLogger().Trace("Initializing AutoMapper ...");

            Mapper.Initialize(
                configurationExpression =>
                {
                    foreach (Type profile in GetAS4MappingProfiles())
                    {
                        configurationExpression.AddProfile(profile);
                    }
                });

            Mapper.Configuration.AssertConfigurationIsValid();

            LogManager.GetCurrentClassLogger().Trace("AutoMapper initialized.");
        }

        /// <summary>
        /// Map/Project the given source Model to a given Destination Model
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination Map<TDestination>(object source)
        {
            return Mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// Map/Project the given source Model to a given Destination Model
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="setMappingOptions"></param>
        /// <returns></returns>
        public static TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> setMappingOptions)
        {
            return Mapper.Map<TDestination>(source, setMappingOptions);
        }
    }
}