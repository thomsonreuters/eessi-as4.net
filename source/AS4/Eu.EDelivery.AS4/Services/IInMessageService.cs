using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Services
{
    public interface IInMessageService
    {
        /// <summary>
        /// Inserts an <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="as4Message">The as4 message.</param>
        /// <param name="messageBodyStore">The message body store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task InsertAS4Message(AS4Message as4Message, IAS4MessageBodyStore messageBodyStore, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an <see cref="AS4Message"/> for delivery and notification.
        /// </summary>
        /// <param name="as4Message">The as4 message.</param>
        /// <param name="messageBodyStore">The message body store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task UpdateAS4MessageForDeliveryAndNotification(AS4Message as4Message, IAS4MessageBodyStore messageBodyStore, CancellationToken cancellationToken);

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
    }
}