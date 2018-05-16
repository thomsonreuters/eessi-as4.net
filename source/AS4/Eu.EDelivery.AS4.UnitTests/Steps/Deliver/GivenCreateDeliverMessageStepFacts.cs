using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.UnitTests.Model;
using Xunit;
using Party = Eu.EDelivery.AS4.Model.Common.Party;
using Service = Eu.EDelivery.AS4.Model.Common.Service;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="CreateDeliverEnvelopeStep"/>
    /// </summary>
    public class GivenCreateDeliverMessageStepFacts
    {
        [Fact]
        public async Task ThenDeliverMessageIsMapped_IfAS4MessageIsSet()
        {
            // Arrange
            AS4Message as4Message = AS4MessageWithUserMessage();

            // Act
            DeliverMessageEnvelope deliverEvelope = await ExecuteStepWith(as4Message);

            // Assert
            string expectedId = as4Message.GetPrimaryMessageId();
            string actualId = deliverEvelope.MessageInfo.MessageId;

            Assert.Equal(expectedId, actualId);
        }

        [Fact]
        public async Task ThenDeliverMessageIsSerialized_IfAS4MessageIsSet()
        {
            // Arrange
            AS4Message as4Message = AS4MessageWithUserMessage();

            // Act
            DeliverMessageEnvelope deliverEnvelope = await ExecuteStepWith(as4Message);

            // Assert
            Assert.Equal("application/xml", deliverEnvelope.ContentType);
            Assert.NotEmpty(deliverEnvelope.DeliverMessage);
        }

        [Fact]
        public async Task ThenAgreementRefIsMappedCorrectly_IfAS4MessageIsSet()
        {
            // Act
            DeliverMessage deliverMessage = await TestExecuteStepWithFullBlownUserMessage();

            // Assert
            Agreement actualAgreement = deliverMessage.CollaborationInfo.AgreementRef;
            Assert.NotEmpty(actualAgreement.Value);
            Assert.NotNull(actualAgreement.PModeId);
        }

        [Fact]
        public async Task ThenServiceIsMappedCorrectly_IfAS4MessageIsSet()
        {
            // Act
            DeliverMessage deliverMessage = await TestExecuteStepWithFullBlownUserMessage();

            // Assert
            Service service = deliverMessage.CollaborationInfo.Service;
            AssertsNotEmpty(service.Type, service.Value);
        }

        [Fact]
        public async Task ThenPartiesAreMappedCorrectly_IfAS4MessageIsSet()
        {
            // Act
            DeliverMessage deliverMessage = await TestExecuteStepWithFullBlownUserMessage();

            // Assert
            Party fromParty = deliverMessage.PartyInfo.FromParty;
            Party toParty = deliverMessage.PartyInfo.ToParty;

            AssertsNotEmpty(fromParty.PartyIds, fromParty.Role, toParty.PartyIds, toParty.Role);
        }

        [Fact]
        public async Task ThenMessagePropertiesAreMappedCorrectly_IfAS4MessageIsSet()
        {
            // Act
            DeliverMessage deliverMessage = await TestExecuteStepWithFullBlownUserMessage();

            // Assert
            Assert.NotEmpty(deliverMessage.MessageProperties);
        }

        [Theory]
        [InlineData("attachment location", "attachment location")]
        [InlineData(null, "")]
        public async Task PartInfoLocationIsAttachmentLocation_IfIdMatches(string attachmentLocation, string expectedLocation)
        {
            // Arrange
            const string referenceId = "payload id";
            AS4Message message = AS4MessageWithUserMessage(referenceId);
            message.AddAttachment(new Attachment(referenceId) {Location = attachmentLocation});

            // Act
            DeliverMessageEnvelope deliverEnvelope = await ExecuteStepWith(message);

            // Assert
            DeliverMessage deliverMessage = DeserializeDeliverEnvelope(deliverEnvelope);
            string actualLocation = deliverMessage.Payloads.First().Location;

            Assert.Equal(expectedLocation, actualLocation);
        }

        [Fact]
        public async Task FailsToCreateDeliverMessage_IfInvalidDeliverMessage()
        {
            // Arrange
            AS4Message as4Message = AS4MessageWithUserMessage();
            as4Message.PrimaryUserMessage.MessageId = null;

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(() => ExecuteStepWith(as4Message));
        }

        private static async Task<DeliverMessage> TestExecuteStepWithFullBlownUserMessage()
        {
            AS4Message as4Message = AS4MessageWithUserMessage();
            DeliverMessageEnvelope deliverEnvelope = await ExecuteStepWith(as4Message);

            return DeserializeDeliverEnvelope(deliverEnvelope);
        }

        private static DeliverMessage DeserializeDeliverEnvelope(DeliverMessageEnvelope deliverEnvelope)
        {
            return AS4XmlSerializer.FromString<DeliverMessage>(
                Encoding.UTF8.GetString(deliverEnvelope.DeliverMessage));
        }

        private static async Task<DeliverMessageEnvelope> ExecuteStepWith(AS4Message as4Message)
        {
            var sut = new CreateDeliverEnvelopeStep();
            StepResult result = await sut.ExecuteAsync(new MessagingContext(as4Message, MessagingContextMode.Unknown));

            return result.MessagingContext.DeliverMessage;
        }

        private static AS4Message AS4MessageWithUserMessage(string attachmentId = "attachment-uri")
        {
            var msg = AS4Message.Create(new FilledUserMessage(attachmentId: attachmentId));
            msg.AddAttachment(new FilledAttachment());

            return msg;
        }

        private static void AssertsNotEmpty(params IEnumerable[] values)
        {
            foreach (IEnumerable value in values)
            {
                Assert.NotEmpty(value);
            }
        }
    }
}
