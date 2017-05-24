using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Services
{
    public interface IInMessageService
    {
        /// <summary>
        /// Search for duplicate <see cref="UserMessage"/> instances in the configured datastore for the given <paramref name="searchedMessageIds"/>.
        /// </summary>
        /// <param name="searchedMessageIds">'EbmsMessageIds' to search for duplicates.</param>
        /// <returns></returns>
        IDictionary<string, bool> DetermineDuplicateUserMessageIds(IEnumerable<string> searchedMessageIds);

        /// <summary>
        /// Search for duplicate <see cref="SignalMessage"/> instances in the configured datastore for the given <paramref name="searchedMessageIds"/>.
        /// </summary>
        /// <param name="searchedMessageIds">'RefToEbmsMessageIds' to search for duplicates.</param>
        /// <returns></returns>
        IDictionary<string, bool> DetermineDuplicateSignalMessageIds(IEnumerable<string> searchedMessageIds);

        Task UpdateAS4MessageForDeliveryAndNotification(
            InternalMessage internalMessage,
            IAS4MessageBodyPersister as4MessageBodyPersister,
            CancellationToken cancellationToken);

        Task InsertAS4Message(
            InternalMessage message,
            IAS4MessageBodyPersister as4MessageBodyPersister,
            CancellationToken cancellationToken);
    }
}