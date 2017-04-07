using System.IO;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// Global Interface to provide a consistent "GetStrategyKey" Method
    /// </summary>
    public interface IPayloadRetriever
    {
        /// <summary>
        /// Retrieve <see cref="Stream"/> contents from a given <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        Task<Stream> RetrievePayloadAsync(string location);
    }
}