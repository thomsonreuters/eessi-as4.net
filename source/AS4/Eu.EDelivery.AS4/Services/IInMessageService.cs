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

        /// <summary>
        /// Update the given message for delivery and notification.
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="as4MessageBodyPersister"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UpdateAS4MessageForDeliveryAndNotification(
            MessagingContext messagingContext,
            IAS4MessageBodyStore as4MessageBodyPersister,
            CancellationToken cancellationToken);

        /// <summary>
        /// Insert the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="as4MessageBodyPersister"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertAS4Message(
            MessagingContext message,
            IAS4MessageBodyStore as4MessageBodyPersister,
            CancellationToken cancellationToken);
    }
}