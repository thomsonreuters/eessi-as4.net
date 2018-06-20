using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Xunit;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using PartyInfo = Eu.EDelivery.AS4.Model.PMode.PartyInfo;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    public class GivenCreateAS4MessageStepFacts
    {
        [Fact]
        public async Task CanCreateMessageWithPModeWithoutToParty()
        {
            // Arrange
            var sendingParty = CreatePModeParty("sender", "c2", "eu.edelivery.services");

            var pmode = CreateSendingPMode(fromParty: sendingParty, toParty: null);

            var receivingParty = CreateSubmitMessageParty("receiver", "", "c3");

            var submitMessage = CreateSubmitMessage(pmode, fromParty: null, toParty: receivingParty);

            var context = new MessagingContext(submitMessage) {SendingPMode = pmode};

            // Act
            var result = await ExerciseCreation(context);

            // Assert
            Assert.True(result.Succeeded);
            AS4Message as4Message = result.MessagingContext.AS4Message;

            Assert.False(as4Message.IsEmpty);
            Assert.True(as4Message.IsUserMessage);
            Assert.Equal(receivingParty.Role, as4Message.FirstUserMessage.Receiver.Role);
            Assert.Equal(receivingParty.PartyIds.First().Id, as4Message.FirstUserMessage.Receiver.PartyIds.First().Id);
            Assert.Equal(receivingParty.PartyIds.First().Type, as4Message.FirstUserMessage.Receiver.PartyIds.First().Type);
        }

        [Fact]
        public async Task CanCreateMessageWithPModeWithoutFromParty()
        {
            // Arrange
            var receivingParty = CreatePModeParty("receiver", "c3", "eu.edelivery.services");

            var pmode = CreateSendingPMode(fromParty: null, toParty: receivingParty);

            var fromParty = CreateSubmitMessageParty("sender", "", "c2");

            var submitMessage = CreateSubmitMessage(pmode, fromParty: fromParty, toParty: null);

            var context = new MessagingContext(submitMessage) {SendingPMode = pmode};

            // Act
            StepResult result = await ExerciseCreation(context);

            // Assert
            Assert.True(result.Succeeded);
            var as4Message = result.MessagingContext.AS4Message;

            Assert.False(as4Message.IsEmpty);
            Assert.True(as4Message.IsUserMessage);
            Assert.Equal(fromParty.Role, as4Message.FirstUserMessage.Sender.Role);
            Assert.Equal(fromParty.PartyIds.First().Id, as4Message.FirstUserMessage.Sender.PartyIds.First().Id);
            Assert.Equal(fromParty.PartyIds.First().Type, as4Message.FirstUserMessage.Sender.PartyIds.First().Type);
        }

        [Fact]
        public async Task MessageIsCreatedWithDefaultSender_IfNoneIsSpecified()
        {
            // Arrange
            var receivingParty = CreatePModeParty("receiver", "c3", "eu.edelivery.services");

            var pmode = CreateSendingPMode(fromParty: null, toParty: receivingParty);

            var submitMessage = CreateSubmitMessage(pmode, fromParty: null, toParty: null);

            var context = new MessagingContext(submitMessage) {SendingPMode = pmode};

            // Act
            var result = await ExerciseCreation(context);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(Constants.Namespaces.EbmsDefaultFrom, result.MessagingContext.AS4Message.FirstUserMessage.Sender.PartyIds.First().Id);
            Assert.Equal(Constants.Namespaces.EbmsDefaultRole, result.MessagingContext.AS4Message.FirstUserMessage.Sender.Role);
        }

        [Fact]
        public async Task MessageIsCreatedWithDefaultReceiver_IfNoneIsSpecified()
        {            
            // Arrange
            var pmode = CreateSendingPMode(fromParty: null, toParty: null);

            var submitMessage = CreateSubmitMessage(pmode, fromParty: null, toParty: null);

            var context = new MessagingContext(submitMessage) {SendingPMode = pmode};

            // Act
            var result = await ExerciseCreation(context);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(Constants.Namespaces.EbmsDefaultTo, result.MessagingContext.AS4Message.FirstUserMessage.Receiver.PartyIds.First().Id);
            Assert.Equal(Constants.Namespaces.EbmsDefaultRole, result.MessagingContext.AS4Message.FirstUserMessage.Receiver.Role);
        }

        [Fact]
        public async Task MessageIsCreatedWithMessageProperties()
        {
            // Arrange
            var pmode = CreateSendingPMode(fromParty: null, toParty: null);

            var submitMessage = CreateSubmitMessage(pmode, fromParty: null, toParty: null);
            submitMessage.MessageProperties = new []
            {
                new AS4.Model.Common.MessageProperty("originalSender","unregistered:C1"),
                new AS4.Model.Common.MessageProperty("finalRecipient","unregistered:C2")
            };

            var context = new MessagingContext(submitMessage) { SendingPMode = pmode };

            // Act
            var result = await ExerciseCreation(context);
            var as4Message = result.MessagingContext.AS4Message;

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(2, as4Message.FirstUserMessage.MessageProperties.Count);
            Assert.Equal("unregistered:C1", as4Message.FirstUserMessage.MessageProperties.FirstOrDefault(p => p.Name.Equals("originalSender"))?.Value);
            Assert.Equal("unregistered:C2", as4Message.FirstUserMessage.MessageProperties.FirstOrDefault(p => p.Name.Equals("finalRecipient"))?.Value);
        }

        [Fact]
        public async Task MessageIsntCreated_IfDuplicatePayloadIdsAreFound()
        {
            // Arrange
            SendingProcessingMode pmode = ValidSendingPModeFactory.Create();
            var submit = new SubmitMessage
            {
                PMode = pmode,
                Payloads = new []
                {
                    new Payload("earth", "location", "mime"),
                    new Payload("earth", "location", "mime")
                }
            };
            var context = new MessagingContext(submit) {SendingPMode = pmode};

            // Act / Assert
            await Assert.ThrowsAsync<InvalidMessageException>(() => ExerciseCreation(context));
        }

        private static Party CreatePModeParty(string role, string id, string type)
        {
            return new Party(role, new PartyId { Id = id, Type = type });
        }

        private static AS4.Model.Common.Party CreateSubmitMessageParty(string role, string type, string id)
        {
            return new AS4.Model.Common.Party { Role = role, PartyIds = new[] { new AS4.Model.Common.PartyId(id, type), } };
        }

        private static SubmitMessage CreateSubmitMessage(
            SendingProcessingMode pmode, 
            AS4.Model.Common.Party fromParty, 
            AS4.Model.Common.Party toParty)
        {
            return new SubmitMessage
            {
                Collaboration =
                {
                    AgreementRef = {PModeId = "not empty pmode id"}
                },
                PartyInfo = new AS4.Model.Common.PartyInfo
                {
                    FromParty = fromParty,
                    ToParty = toParty
                },
                PMode = pmode
            };
        }

        private static SendingProcessingMode CreateSendingPMode(Party fromParty, Party toParty)
        {
            SendingProcessingMode pmode = ValidSendingPModeFactory.Create();

            pmode.MessagePackaging = new SendMessagePackaging
            {
                PartyInfo = new PartyInfo
                {
                    FromParty = fromParty,
                    ToParty = toParty
                }
            };

            return pmode;
        }

        private static async Task<StepResult> ExerciseCreation(MessagingContext context)
        {
            var sut = new CreateAS4MessageStep();
            return await sut.ExecuteAsync(context);
        }
    }
}
