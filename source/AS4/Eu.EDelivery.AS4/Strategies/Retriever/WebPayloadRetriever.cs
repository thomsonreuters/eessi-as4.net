using System;
using System.IO;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// Web Retriever Implementation to retrieve the RequestStream of a external file
    /// </summary>
    public class WebPayloadRetriever : IPayloadRetriever
    {
        /// <summary>
        /// Retrieve the payload from the given location
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        /// <returns>
        /// </returns>
        public Stream RetrievePayload(string location)
        {
            throw new NotImplementedException();
        }
    }
}