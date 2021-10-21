using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Resources;
using Eu.EDelivery.AS4.Serialization;
using log4net;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Adapter to "Adapt" a SubmitMessage > AS4Message
    /// </summary>
    public class SubmitMessageXmlTransformer : ITransformer
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

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
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Logger.Trace("Start deserializing to a SubmitMessage...");
            SubmitMessage submitMessage = DeserializeSubmitMessage(message);
            Logger.Trace("Successfully deserialized to a SubmitMessage");

            return await Task.FromResult(new MessagingContext(submitMessage));
        }

        private static SubmitMessage DeserializeSubmitMessage(ReceivedMessage message)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(message.UnderlyingStream);

                return AS4XmlSerializer.FromString<SubmitMessage>(doc.OuterXml);
            }
            catch (Exception ex)
            {
                throw new InvalidMessageException(
                    $"Received stream from {message.Origin} is not a SubmitMessage", ex);
            }
        }
    }
}