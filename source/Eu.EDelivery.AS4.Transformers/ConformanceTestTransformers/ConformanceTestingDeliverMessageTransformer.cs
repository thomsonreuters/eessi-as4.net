using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using CollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.Transformers.ConformanceTestTransformers
{
    [NotConfigurable]
    public class ConformanceTestingDeliverMessageTransformer : ITransformer
    {
        private string _uriPrefix;

        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties)
        {
            _uriPrefix = properties.ReadMandatoryProperty("Uri");
        }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// 
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            if (!(message is ReceivedEntityMessage))
            {
                throw new NotSupportedException(
                    $"Minder Deliver Transformer only supports transforming instances of type {typeof(ReceivedEntityMessage)}");
            }

            var as4Transformer = new AS4MessageTransformer();
            MessagingContext context = await as4Transformer.TransformAsync(message);

            var includeAttachments = true;
            CollaborationInfo collaborationInfo = context.ReceivingPMode?.MessagePackaging?.CollaborationInfo;

            if (collaborationInfo != null &&
                (collaborationInfo.Action?.Equals("ACT_SIMPLE_ONEWAY_SIZE", StringComparison.OrdinalIgnoreCase) ?? false) &&
                (collaborationInfo.Service?.Value?.Equals("SRV_SIMPLE_ONEWAY_SIZE", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                includeAttachments = false;
            }

            DeliverMessageEnvelope deliverMessage = CreateDeliverMessageEnvelope(context.AS4Message, includeAttachments);
            context.ModifyContext(deliverMessage);

            return context;
        }

        private DeliverMessageEnvelope CreateDeliverMessageEnvelope(AS4Message as4Message, bool includeAttachments)
        {
            UserMessage deliverMessage = CreateMinderDeliverMessage(as4Message);

            // The Minder Deliver Message should be an AS4-Message.
            AS4Message msg = AS4Message.Create(deliverMessage);

            if (includeAttachments)
            {
                msg.AddAttachments(as4Message.Attachments);
            }

            byte[] content = SerializeAS4Message(msg);

            return new DeliverMessageEnvelope(
                messageInfo: new MessageInfo
                {
                    MessageId = deliverMessage.MessageId,
                    RefToMessageId = deliverMessage.RefToMessageId
                },
                deliverMessage: content,
                contentType: msg.ContentType,
                attachments: as4Message.UserMessages.SelectMany(um => as4Message.Attachments.Where(a => a.MatchesAny(um.PayloadInfo))));
        }

        private static byte[] SerializeAS4Message(AS4Message msg)
        {
            ISerializer serializer = SerializerProvider.Default.Get(msg.ContentType);

            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(msg, memoryStream);
                return memoryStream.ToArray();
            }
        }

        private UserMessage CreateMinderDeliverMessage(AS4Message as4Message)
        {
            UserMessage userMessage = as4Message.FirstUserMessage;

            // Party Information: sender is the receiver of the AS4Message that has been received.
            //                    receiver is minder.

            IEnumerable<MessageProperty> deliverProperties =
                new Dictionary<string, string>
                {
                    ["MessageId"] = userMessage.MessageId,
                    ["RefToMessageId"] = userMessage.RefToMessageId,
                    ["ConversationId"] = userMessage.CollaborationInfo.ConversationId,
                    ["Service"] = userMessage.CollaborationInfo.Service.Value,
                    ["Action"] = userMessage.CollaborationInfo.Action,
                    ["FromPartyId"] = userMessage.Sender.PartyIds.First().Id,
                    ["FromPartyRole"] = userMessage.Sender.Role,
                    ["ToPartyId"] = userMessage.Receiver.PartyIds.First().Id,
                    ["ToPartyRole"] = userMessage.Receiver.Role
                }
                .Select(kv => new MessageProperty(kv.Key, kv.Value))
                .Concat(userMessage.MessageProperties.Where(p => p.Name.Equals("originalSender") || p.Name.Equals("finalRecipient")))
                .ToArray();

            return new UserMessage(
                messageId: userMessage.MessageId,
                refToMessageId: userMessage.RefToMessageId,
                timestamp: userMessage.Timestamp,
                mpc: Constants.Namespaces.EbmsDefaultMpc,
                collaboration: new Model.Core.CollaborationInfo(
                    agreement: Maybe<AgreementReference>.Nothing,
                    service: new Service(_uriPrefix),
                    action: "Deliver",
                    conversationId: userMessage.CollaborationInfo.ConversationId),
                sender: new Party($"{_uriPrefix}/sut", userMessage.Receiver.PartyIds.FirstOrDefault()),
                receiver: new Party($"{_uriPrefix}/testdriver", new PartyId("minder")),
                partInfos: userMessage.PayloadInfo,
                messageProperties: deliverProperties);
        }
    }
}
