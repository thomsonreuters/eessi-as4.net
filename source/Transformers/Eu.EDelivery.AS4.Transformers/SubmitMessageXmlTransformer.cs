using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Adapter to "Adapt" a SubmitMessage > AS4Message
    /// </summary>
    public class SubmitMessageXmlTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a <see cref="SubmitMessage" />
        /// to a <see cref="MessagingContext"/>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            Logger.Info("Transforming ReceivedMessage to MessagingContext");

            SubmitMessage submitMessage = DeserializeSubmitMessage(message.UnderlyingStream);

            return await Task.FromResult(new MessagingContext(submitMessage));
        }

        private static SubmitMessage DeserializeSubmitMessage(Stream stream)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SubmitMessage));

                return serializer.Deserialize(stream) as SubmitMessage;
            }
            catch (Exception ex)
            {
                throw new InvalidMessageException("Received stream is not a SubmitMessage", ex);
            }
        }


    }
}