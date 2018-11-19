using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="CreateDeliverEnvelopeStep"/>
    /// </summary>
    public class GivenCreateDeliverMessageStepFacts
    {
        [Fact]
        public async Task Create_DeliverMessage_From_UserMessage()
        {
            // Arrange
            string partInfoId = $"part-{Guid.NewGuid()}";
            var userMessage = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new CollaborationInfo(
                    new Service($"service-{Guid.NewGuid()}"),
                    $"action-{Guid.NewGuid()}"),
                new Party("Sender", new PartyId($"id-{Guid.NewGuid()}")), 
                new Party("Receiver", new PartyId($"id-{Guid.NewGuid()}")),
                new [] { new PartInfo($"cid:{partInfoId}") },
                new MessageProperty[0]);

            AS4Message as4Message = AS4Message.Create(userMessage);
            as4Message.AddAttachment(new Attachment(partInfoId) { Location = $"location-{Guid.NewGuid()}" });

            var ctx = new MessagingContext(as4Message, MessagingContextMode.Deliver)
            {
                ReceivingPMode = new ReceivingProcessingMode { Id = "deliver-pmode" }
            };

            var sut = new CreateDeliverEnvelopeStep();

            // Act
            StepResult result = await sut.ExecuteAsync(ctx);

            // Assert
            var deliverMessage =
                AS4XmlSerializer.FromString<DeliverMessage>(
                    Encoding.UTF8.GetString(result.MessagingContext.DeliverMessage.DeliverMessage));

            IEnumerable<string> mappingFailures = 
                DeliverMessageOriginateFrom(
                    userMessage,
                    as4Message.Attachments,
                    ctx.ReceivingPMode, 
                    deliverMessage);

            Assert.Empty(mappingFailures);
        }

        private static IEnumerable<string> DeliverMessageOriginateFrom(
            UserMessage user,
            IEnumerable<Attachment> attachments,
            ReceivingProcessingMode receivingPMode, 
            DeliverMessage deliver)
        {
            if (user.MessageId != deliver.MessageInfo?.MessageId)
            {
                yield return "MessageId";
            }

            if (user.CollaborationInfo.Service.Value != deliver.CollaborationInfo?.Service?.Value)
            {
                yield return "Service";
            }

            if (user.CollaborationInfo.Action != deliver.CollaborationInfo?.Action)
            {
                yield return "Action";
            }

            if (user.Sender.PrimaryPartyId != deliver.PartyInfo?.FromParty?.PartyIds?.FirstOrDefault()?.Id)
            {
                yield return "FromParty";
            }

            if (user.Receiver.PrimaryPartyId != deliver.PartyInfo?.ToParty?.PartyIds?.FirstOrDefault()?.Id)
            {
                yield return "ToParty";
            }

            if (receivingPMode.Id != deliver.CollaborationInfo?.AgreementRef?.PModeId)
            {
                yield return "PModeId";
            }

            if (attachments.First().Location != deliver.Payloads?.FirstOrDefault()?.Location)
            {
                yield return "AttachmentLocation";
            }
        }
    }
}
