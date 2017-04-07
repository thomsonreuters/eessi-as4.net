using System;

namespace Eu.EDelivery.AS4.UnitTests.Http
{
    public struct UniqueHost
    {
        public string Url { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueHost"/> struct. 
        /// </summary>
        /// <param name="url"></param>
        private UniqueHost(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Create a new instance of the <see cref="UniqueHost"/> class.
        /// </summary>
        /// <returns></returns>
        public static UniqueHost Create()
        {
            return new UniqueHost($"http://localhost:{new Random().Next(0, 9999)}");
        }
    }
}
