using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    internal class StubMessageBodyPersister : IAS4MessageBodyPersister
    {
        internal static StubMessageBodyPersister Default = new StubMessageBodyPersister();

        public Task<string> SaveAS4MessageAsync(AS4Message message, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }

        public Task UpdateAS4MessageAsync(string location, AS4Message message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
