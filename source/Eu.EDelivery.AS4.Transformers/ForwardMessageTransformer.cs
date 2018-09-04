using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    public class ForwardMessageTransformer : ITransformer
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
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            if (message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ForwardMessageTransformer)} requires a ReceivedMessage to wrap into a MessagingContext");
            }

            var context = new MessagingContext(message, MessagingContextMode.Forward);
            message.AssignPropertiesTo(context);

            return await Task.FromResult(context);
        }
    }
}