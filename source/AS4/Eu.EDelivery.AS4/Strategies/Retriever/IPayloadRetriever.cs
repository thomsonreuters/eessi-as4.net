using System.IO;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// Global Interface to provide a consistent "GetStrategyKey" Method
    /// </summary>
    public interface IPayloadRetriever
    {
        Stream RetrievePayload(string location);
    }
}