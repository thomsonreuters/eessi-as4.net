using System.IO;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    /// <summary>
    /// <see cref="IAS4MessageBodyRetriever"/> implementation to sabotage the loading of a <see cref="Stream"/> at a given location.
    /// </summary>
    public class SaboteurMessageBodyRetriever : IAS4MessageBodyRetriever
    {
        /// <summary>
        /// Loads a <see cref="Stream"/> at a given stored <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location on which the <see cref="Stream"/> is stored.</param>
        /// <returns></returns>
        public Stream LoadAS4MessageStream(string location)
        {
            throw new SaboteurException("Sabotage the load of AS4 Messages");
        }
    }
}
