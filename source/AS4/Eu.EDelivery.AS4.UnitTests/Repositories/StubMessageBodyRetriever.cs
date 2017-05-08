using System.IO;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    /// <summary>
    /// <see cref="IAS4MessageBodyPersister"/> implementation to return a 'stubbed' <see cref="Stream"/>.
    /// </summary>
    public class StubMessageBodyRetriever : IAS4MessageBodyRetriever
    {
        /// <summary>
        /// Loads a <see cref="Stream"/> at a given stored <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location on which the <see cref="Stream"/> is stored.</param>
        /// <returns></returns>
        public Stream LoadAS4MessageStream(string location)
        {
            return Stream.Null;
        }
    }
}
