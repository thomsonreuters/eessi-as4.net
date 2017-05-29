using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Services
{
    public interface IReceptionAwarenessService
    {
        /// <summary>
        /// Deadletters the out message asynchronous.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="messageBodyPersister">The message body persister.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task DeadletterOutMessageAsync(string messageId, IAS4MessageBodyPersister messageBodyPersister, CancellationToken cancellationToken);

        /// <summary>
        /// Messages the needs to be resend.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        /// <returns></returns>
        bool MessageNeedsToBeResend(ReceptionAwareness awareness);
    }
}