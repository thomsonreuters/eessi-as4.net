using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    public class ForwardMessageTransformer : ITransformer
    {

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var context = new MessagingContext(message, MessagingContextMode.Forward);
            message.AssignPropertiesTo(context);

            return await Task.FromResult(context);
        }
    }
}