using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Eu.EDelivery.AS4.Mappings.Core;

namespace Eu.EDelivery.AS4.Mappings
{
    /// <summary>
    /// Initialize Mappings for the <see cref="Core"/> namespace
    /// </summary>
    internal static class MapInitialization
    {   
        private static readonly object InitializeLock = new object();

        internal static void InitializeMapper()
        {
            lock (InitializeLock)
            {
                Mapper.Initialize(AddProfiles);
                Mapper.Configuration.AssertConfigurationIsValid();
            }
        }

        private static void AddProfiles(IMapperConfigurationExpression configurationExpression)
        {
            IEnumerable<Type> profiles =
                typeof(PullRequestMap).Assembly.GetTypes().Where(x => typeof(Profile).IsAssignableFrom(x));

            foreach (Type profile in profiles)
                configurationExpression.AddProfile(profile);
        }
    }
}