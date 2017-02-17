using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Assemble a <see cref="AS4Message"/> as Deliver Message
    /// </summary>
    public class MinderCreateDeliverMessageStep : IStep
    {
        // TODO: this Step should be replaced by a Transformer.

        private const string ConformanceUriPrefix = "http://www.esens.eu/as4/conformancetest";
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderCreateDeliverMessageStep"/>
        /// </summary>
        public MinderCreateDeliverMessageStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._logger.Info("Minder Create Deliver Message");

            var deliverMessage = CreateMinderDeliverMessage(internalMessage);

            // The Minder Deliver Message should be an AS4-Message.
            var builder = new AS4MessageBuilder();

            builder.WithUserMessage(deliverMessage);

            foreach (var att in internalMessage.AS4Message.Attachments)
            {
                builder.WithAttachment(att);
            }

            var msg = builder.Build();

            var serializer = Registry.Instance.SerializerProvider.Get(msg.ContentType);

            byte[] content;

            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(msg, memoryStream, CancellationToken.None);
                content = memoryStream.ToArray();
            }


            internalMessage.DeliverMessage = new DeliverMessageEnvelope(new MessageInfo()
            {
                MessageId = deliverMessage.MessageId,
                RefToMessageId = deliverMessage.RefToMessageId
            },
                content,
                msg.ContentType);

            return StepResult.SuccessAsync(internalMessage);
        }

        private UserMessage CreateMinderDeliverMessage(InternalMessage internalMessage)
        {
            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;

            UserMessage deliverMessage = new UserMessage(userMessage.MessageId);
            deliverMessage.RefToMessageId = userMessage.RefToMessageId;
            deliverMessage.Timestamp = userMessage.Timestamp;

            deliverMessage.CollaborationInfo.Action = "Deliver";
            deliverMessage.CollaborationInfo.Service.Value = ConformanceUriPrefix;
            deliverMessage.CollaborationInfo.ConversationId = userMessage.CollaborationInfo.ConversationId;

            // Party Information: sender is the receiver of the AS4Message that has been received.
            //                    receiver is minder.
            deliverMessage.Sender = new Party($"{ConformanceUriPrefix}/sut", userMessage.Receiver.PartyIds.FirstOrDefault());
            deliverMessage.Receiver = new Party($"{ConformanceUriPrefix}/testdriver", new PartyId("minder"));

            // Set the PayloadInfo.
            foreach (var pi in userMessage.PayloadInfo)
            {
                deliverMessage.PayloadInfo.Add(pi);
            }

            deliverMessage.MessageProperties.Add(new MessageProperty("MessageId", userMessage.MessageId));
            AddMessageProperty(deliverMessage, "RefToMessageId", userMessage.RefToMessageId);
            AddMessageProperty(deliverMessage, "ConversationId", userMessage.CollaborationInfo.ConversationId);

            AddMessageProperty(deliverMessage, "Service", userMessage.CollaborationInfo.Service.Value);
            AddMessageProperty(deliverMessage, "Action", userMessage.CollaborationInfo.Action);
            AddMessageProperty(deliverMessage, "ConversationId", userMessage.CollaborationInfo.ConversationId);

            AddMessageProperty(deliverMessage, "FromPartyId", userMessage.Sender.PartyIds.First().Id);
            AddMessageProperty(deliverMessage, "FromPartyRole", userMessage.Sender.Role);

            AddMessageProperty(deliverMessage, "ToPartyId", userMessage.Receiver.PartyIds.First().Id);
            AddMessageProperty(deliverMessage, "ToPartyRole", userMessage.Receiver.Role);

            AddMessageProperty(deliverMessage, userMessage.MessageProperties.FirstOrDefault(p => p.Name.Equals("originalSender")));
            AddMessageProperty(deliverMessage, userMessage.MessageProperties.FirstOrDefault(p => p.Name.Equals("finalRecipient")));

            return deliverMessage;
        }

        private void AddMessageProperty(UserMessage message, string propertyName, string propertyValue)
        {
            if (propertyValue == null)
            {
                return;
            }

            AddMessageProperty(message, new MessageProperty(propertyName, propertyValue));
        }

        private void AddMessageProperty(UserMessage message, MessageProperty property)
        {
            if (property == null)
            {
                return;
            }
            message.MessageProperties.Add(property);
        }

    }
}