using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    public class OutMessageTransformer : ITransformer
    {
        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var context = new MessagingContext(message, MessagingContextMode.Send);
            message.AssignPropertiesTo(context);

            return await Task.FromResult(context);
        }
    }
}