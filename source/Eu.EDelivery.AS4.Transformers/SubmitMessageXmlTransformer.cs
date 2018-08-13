using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Resources;
using Eu.EDelivery.AS4.Serialization;
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

                var schemas = new XmlSchemaSet();
                schemas.Add(XsdSchemaDefinitions.SubmitMessage);
                doc.Schemas = schemas;

                doc.Validate((sender, args) =>
                {
                    Logger.Fatal("Incoming Submit Message doesn't match the XSD: " + args.Message);
                    throw args.Exception;
                });

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