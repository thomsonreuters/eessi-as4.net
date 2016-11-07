using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Mappings
{
    internal static class MapInitialization
    {
        private static object initializeLock = new object();
        private static bool isInitialized = false;

        internal static void InitializeMapper()
        {
            lock (initializeLock)
            {
                if (!isInitialized)
                {
                    Mapper.Initialize(cfg =>
                    {
                        var profiles = typeof(PullRequestMap).Assembly.GetTypes().Where(x => typeof(Profile).IsAssignableFrom(x));
                        foreach (Type profile in profiles)
                        {
                            cfg.AddProfile(profile);
                        }
                    });

                    Mapper.Configuration.AssertConfigurationIsValid();
                    isInitialized = true;
                }
            }
        }
    }
}
