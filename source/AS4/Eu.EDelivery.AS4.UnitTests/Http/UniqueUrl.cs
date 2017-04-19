using System;

namespace Eu.EDelivery.AS4.UnitTests.Http
{
    public static class UniqueHost
    {
        private static readonly Random Random;

        static UniqueHost()
        {
            Random = new Random();
        }

        /// <summary>
        /// Create a new instance of the <see cref="UniqueHost" /> class.
        /// </summary>
        /// <returns></returns>
        public static string Create()
        {
            return $"http://localhost:{Random.Next(4800, 4900)}";
        }
    }
}