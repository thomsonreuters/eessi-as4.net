using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Eu.EDelivery.AS4.UnitTests.Http
{
    public static class UniqueHost
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Create a new instance of the <see cref="UniqueHost" /> class.
        /// </summary>
        /// <returns></returns>
        public static string Create()
        {
            return $"http://localhost:{GetOpenPort()}";
        }

        private static int GetOpenPort(int port = -1)
        {
            if (!IsInUse(port) && port != -1)
            {
                return port;
            }

            return GetOpenPort(Random.Next(4700, 4800));
        }

        public static bool IsInUse(int port)
        {
            List<int> usedPorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port).ToList();

            return usedPorts.Contains(port);
        }
    }
}