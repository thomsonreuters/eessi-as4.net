using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Mappings.Submit;
using NLog;

namespace Eu.EDelivery.AS4.Singletons
{
    /// <summary>
    /// Singleton to expose Internal Mapper
    /// </summary>
    public static class AS4Mapper
    {
        static AS4Mapper()
        {
            LogManager.GetCurrentClassLogger().Trace("Initializing AutoMapper ...");

            // static constructor is guaranteed to be executed only once, which makes it the ideal place
            // to initialize the Automapper-mappings.            
            Mapper.Initialize(
                configurationExpression =>
                {
                    IEnumerable<Type> profiles = typeof(AS4Mapper).Assembly.GetTypes().Where(x => typeof(Profile).IsAssignableFrom(x));

                    foreach (Type profile in profiles)
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
    }
}