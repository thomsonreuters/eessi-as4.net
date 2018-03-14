using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
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
        /// Inserts a received Message in the DataStore.
        /// For each message-unit that exists in the AS4Message,an InMessage record is created.
        /// The AS4 Message Body is persisted as it has been received.
        /// </summary>
        /// <remarks>The received Message is parsed to an AS4 Message instance.</remarks>
        /// <param name="context"></param>
        /// <param name="mep">The MessageExchangePattern by which the message has been received</param>
        /// <param name="as4MessageBodyPersister"></param>
        /// <returns>A MessagingContext instance that contains the parsed AS4 Message.</returns>
        Task<MessagingContext> InsertAS4MessageAsync(MessagingContext context, MessageExchangePattern mep, IAS4MessageBodyStore as4MessageBodyPersister);

        /// <summary>
        /// Update the given message for delivery and notification.
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="as4MessageBodyPersister"></param>
        /// <returns></returns>
        void UpdateAS4MessageForMessageHandling(MessagingContext messagingContext, IAS4MessageBodyStore as4MessageBodyPersister);
    }
}