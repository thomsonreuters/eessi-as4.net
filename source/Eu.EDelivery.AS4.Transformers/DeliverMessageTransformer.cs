using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using log4net;

namespace Eu.EDelivery.AS4.Transformers
{
    public class DeliverMessageTransformer : ITransformer
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
        private static readonly AS4MessageTransformer AS4MessageTransformer = new AS4MessageTransformer();

        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage" /> to a Canonical <see cref="MessagingContext" /> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (!(message is ReceivedEntityMessage entityMessage) 
                || !(entityMessage.Entity is MessageEntity me))
            {
                throw new InvalidDataException(
                    $"The message that must be transformed should be of type {nameof(ReceivedEntityMessage)} with a {nameof(MessageEntity)} as Entity");
            }

            MessagingContext context = await AS4MessageTransformer.TransformAsync(entityMessage);
            AS4Message as4Message = context.AS4Message;

            UserMessage toBeDeliveredUserMessage = 
                as4Message.UserMessages.FirstOrDefault(u => u.MessageId == me.EbmsMessageId);

            if (toBeDeliveredUserMessage == null)
            {
                throw new InvalidOperationException(
                    $"No UserMessage {me.EbmsMessageId} can be found in stored record for delivering");
            }

            IEnumerable<Attachment> toBeUploadedAttachments =
                as4Message.Attachments
                          .Where(a => a.MatchesAny(toBeDeliveredUserMessage.PayloadInfo))
                          .ToArray();

            DeliverMessage deliverMessage =
                CreateDeliverMessage(
                    toBeDeliveredUserMessage,
                    toBeUploadedAttachments,
                    context.ReceivingPMode);

            Logger.Info($"(Deliver) Created DeliverMessage from (first) UserMessage {Config.Encode(as4Message.FirstUserMessage.MessageId)}");
            var envelope = new DeliverMessageEnvelope(
                message: deliverMessage,
                contentType: "application/xml",
                attachments: toBeUploadedAttachments);

           context.ModifyContext(envelope);
            return context;
        }

        public static DeliverMessage CreateDeliverMessage(
            UserMessage user,
            IEnumerable<Attachment> attachments,
            ReceivingProcessingMode receivingPMode)
        {
            if (!attachments.All(a => a.MatchesAny(user.PayloadInfo)))
            {
                throw new InvalidOperationException(
                    "Not all attachments in AS4Message references to an <PartInfo/> element");
            }

            return new DeliverMessage
            {
                MessageInfo =
                {
                    MessageId = user.MessageId,
                    RefToMessageId = user.RefToMessageId,
                    Mpc = user.Mpc
                },
                CollaborationInfo =
                {
                    Action = user.CollaborationInfo.Action,
                    ConversationId = user.CollaborationInfo.ConversationId,
                    AgreementRef = { PModeId = receivingPMode?.Id },
                    Service =
                    {
                        Type = user.CollaborationInfo.Service.Type.GetOrElse(() => null),
                        Value = user.CollaborationInfo.Service.Value
                    }
                },
                PartyInfo =
                {
                    FromParty = CreateDeliverParty(user.Sender),
                    ToParty = CreateDeliverParty(user.Receiver)
                },
                MessageProperties = user.MessageProperties.Select(CreateDeliverMessageProperty).ToArray(),
                Payloads = user.PayloadInfo.Select(CreateDeliverPayload).ToArray()
            };
        }

        private static Model.Common.Party CreateDeliverParty(Model.Core.Party p)
        {
            return new Model.Common.Party
            {
                Role = p.Role,
                PartyIds = p.PartyIds.Select(id => new Model.Common.PartyId(id.Id, id.Type.GetOrElse(() => null))).ToArray()
            };
        }

        private static Model.Common.MessageProperty CreateDeliverMessageProperty(Model.Core.MessageProperty p)
        {
            return new Model.Common.MessageProperty(p.Name, p.Value) { Type = p.Type };
        }

        private static Payload CreateDeliverPayload(PartInfo part)
        {
            return new Payload
            {
                Id = part.Href,
                MimeType = part.HasMimeType ? part.MimeType : null,
                PayloadProperties = part.Properties.Select(p => new PayloadProperty(p.Key, p.Value)).ToArray()
            };
        }
    }
}