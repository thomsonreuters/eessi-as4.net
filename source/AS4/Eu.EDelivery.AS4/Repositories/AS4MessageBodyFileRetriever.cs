using System;
using System.IO;

namespace Eu.EDelivery.AS4.Repositories
{
    internal class AS4MessageBodyFileRetriever : IAS4MessageBodyRetriever
    {
        public Stream LoadAS4MessageStream(string location)
        {
            if (location.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                location = location.Substring("file://".Length);
            }

            return File.OpenRead(location);
        }
    }

    public interface IAS4MessageBodyRetriever
    {
        Stream LoadAS4MessageStream(string location);
    }

}