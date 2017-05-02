using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    internal class StubMessageBodyPersister : IAS4MessageBodyPersister
    {
        internal static StubMessageBodyPersister Default = new StubMessageBodyPersister();

        public string SaveAS4Message(AS4Message message, CancellationToken cancellationToken)
        {
            return string.Empty;
        }
    }
}
