using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using NLog;
using MessageProperty = Eu.EDelivery.AS4.Model.Common.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Common.Party;
using PartyId = Eu.EDelivery.AS4.Model.Common.PartyId;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// <see cref="IStep" /> implementation to create a <see cref="DeliverMessage" />.
    /// </summary>
    [Description("Step that creates a deliver message")]
    [Info("Create deliver message")]
    public class CreateDeliverEnvelopeStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CreateDeliverEnvelopeStep)} requires an AS4Message to create a DeliverMessage from "
                    + "but no AS4Message is present in the MessagingContext");
            }

            if (messagingContext.ReceivingPMode == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CreateDeliverEnvelopeStep)} requires a ReceivingPMode which the DeliverMessage will reference to "
                    + "but no SendingPMode is present in the MessagingContext");
            }

            if (!messagingContext.AS4Message.HasUserMessage)
            {
                throw new InvalidOperationException(
                    $"{nameof(CreateDeliverEnvelopeStep)} requires an AS4Message with at least one UserMessage to create a DeliverMessage");
            }

            AS4Message as4Message = messagingContext.AS4Message;
            DeliverMessage deliverMessage = 
                CreateDeliverMessage(
                    as4Message.FirstUserMessage, 
                    as4Message.Attachments,
                    messagingContext.ReceivingPMode);

            Logger.Info($"(Deliver) Created DeliverMessage from (first) UserMessage {as4Message.FirstUserMessage.MessageId}");

            string serialized = await AS4XmlSerializer.ToStringAsync(deliverMessage);
            var envelope = new DeliverMessageEnvelope(
                messageInfo: deliverMessage.MessageInfo,
                deliverMessage: Encoding.UTF8.GetBytes(serialized),
                contentType: "application/xml");

            messagingContext.ModifyContext(envelope);
            return StepResult.Success(messagingContext);
        }

        private static DeliverMessage CreateDeliverMessage(
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
                    AgreementRef = { PModeId = receivingPMode.Id },
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
                Payloads = user.PayloadInfo.Select(p => CreateDeliverPayload(p, attachments.First(a => a.Matches(p)))).ToArray()
            };
        }

        private static Party CreateDeliverParty(Model.Core.Party p)
        {
            return new Party
            {
                Role = p.Role,
                PartyIds = p.PartyIds.Select(id => new PartyId(id.Id, id.Type.GetOrElse(() => null))).ToArray()
            };
        }

        private static MessageProperty CreateDeliverMessageProperty(Model.Core.MessageProperty p)
        {
            return new MessageProperty(p.Name, p.Value) { Type = p.Type };
        }

        private static Payload CreateDeliverPayload(PartInfo part, Attachment attachment)
        {
            return new Payload
            {
                Id = part.Href,
                Location = attachment.Location,
                MimeType = part.HasMimeType ? part.MimeType : null,
                PayloadProperties = part.Properties.Select(p => new PayloadProperty(p.Key, p.Value)).ToArray()
            };
        }
    }
}