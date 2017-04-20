using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Eu.EDelivery.AS4.UnitTests.Http
{
    public static class UniqueHost
    {
        /// <summary>
        /// Create a new instance of the <see cref="UniqueHost" /> class.
        /// </summary>
        /// <returns></returns>
        public static string Create()
        {
            return $"http://localhost:{GetOpenPort()}";
        }

        public static int GetOpenPort(int startPort = 2555)
        {
            List<int> usedPorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port).ToList();

            return Enumerable.Range(startPort, 99).FirstOrDefault(port => !usedPorts.Contains(port));
        }
    }
}