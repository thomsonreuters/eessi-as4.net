using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Moq;
using Xunit;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyInfo = Eu.EDelivery.AS4.Model.Common.PartyInfo;
using Service = Eu.EDelivery.AS4.Model.Core.Service;
using SubmitMessageProperty = Eu.EDelivery.AS4.Model.Common.MessageProperty;
using UserMessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="CreateAS4MessageStep" />
    /// </summary>
    public class GivenCreateAS4MessageStepFacts
    {
        public class GivenValidArguments : GivenCreateAS4MessageStepFacts
        {
            [Fact]
            public async Task NoPayloadsToRetrieve()
            {
                // Arrange
                SubmitMessage submit = SubmitWithTwoPayloads();
                submit.PMode = DefaultSendPMode();
                submit.Payloads = null;

                var context = new MessagingContext(submit);

                // Act
                StepResult result = await ExerciseCreateAS4Message(context);

                // Assert
                AS4Message actual = result.MessagingContext.AS4Message;
                Assert.False(actual.HasAttachments);
            }

            [Fact]
            public async Task AssignsAttachmentLocations()
            {
                // Arrange
                SubmitMessage message = SubmitWithTwoPayloads();
                message.PMode = DefaultSendPMode();

                var context = new MessagingContext(message);

                // Act
                StepResult result = await ExerciseCreateAS4Message(context);

                // Assert
                AS4Message actual = result.MessagingContext.AS4Message;
                Assert.True(actual.HasAttachments);
                Assert.Equal(Stream.Null, actual.Attachments.First().Content);
            }

            [Fact]
            public async Task ThenStepCreatesAS4Message()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PMode = DefaultSendPMode();
                var context = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(context);

                // Assert
                UserMessage userMessage = result.MessagingContext.AS4Message.FirstUserMessage;
                Assert.NotNull(userMessage);
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithMessageInfo()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(internalMessage);

                // Assert
                MessageInfo submitMessageInfo = submitMessage.MessageInfo;
                UserMessage userMessage = result.MessagingContext.AS4Message.FirstUserMessage;
                Assert.Equal(submitMessageInfo.MessageId, userMessage.MessageId);
                Assert.Equal(submitMessageInfo.Mpc, userMessage.Mpc);
                Assert.Equal(submitMessageInfo.RefToMessageId, userMessage.RefToMessageId);
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithMpcFromSubmitMessage()
            {
                SubmitMessage submitMessage = CreateSubmitMessageWithMpc("some-mpc");
                submitMessage.PMode = DefaultSendPMode();
                submitMessage.Collaboration.AgreementRef.PModeId = submitMessage.PMode.Id;
                submitMessage.PMode.AllowOverride = true;
                var context = new MessagingContext(submitMessage);

                StepResult result = await ExerciseCreateAS4Message(context);

                Assert.Equal(result.MessagingContext.AS4Message.FirstUserMessage.Mpc, submitMessage.MessageInfo.Mpc);
            }

            private static SubmitMessage CreateSubmitMessageWithMpc(string mpc)
            {
                var message = new SubmitMessage();                
                
                message.MessageInfo.Mpc = mpc;

                return message;
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithMpcFromSendingPMode()
            {
                SubmitMessage submitMessage = new SubmitMessage();
                submitMessage.PMode = DefaultSendPMode();
                submitMessage.Collaboration.AgreementRef.PModeId = submitMessage.PMode.Id;
                submitMessage.PMode.MessagePackaging.Mpc = "some-mpc";

                var context = new MessagingContext(submitMessage);

                StepResult result = await ExerciseCreateAS4Message(context);

                Assert.Equal("some-mpc", result.MessagingContext.AS4Message.FirstUserMessage.Mpc );
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithGeneratedMessageId()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.MessageInfo.MessageId = null;
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(internalMessage);

                // Assert
                Assert.NotEmpty(result.MessagingContext.AS4Message.FirstUserMessage.MessageId);
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithAgreement()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(internalMessage);

                // Assert
                AssertAgreementReference(submitMessage, result.MessagingContext);
            }

            private static void AssertAgreementReference(SubmitMessage submitMessage, MessagingContext messagingContext)
            {
                AgreementReference pmodeAgreementRef =
                    submitMessage.PMode.MessagePackaging.CollaborationInfo.AgreementReference;
                AgreementReference userMessageAgreementRef =
                    messagingContext.AS4Message.FirstUserMessage.CollaborationInfo.AgreementReference;

                Assert.Equal(pmodeAgreementRef.Value, userMessageAgreementRef.Value);
                Assert.Equal(pmodeAgreementRef.Type, userMessageAgreementRef.Type);
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithService()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(internalMessage);

                // Assert
                Service pmodService = submitMessage.PMode.MessagePackaging.CollaborationInfo.Service;
                Service userMessageService = result.MessagingContext.AS4Message.FirstUserMessage.CollaborationInfo.Service;
                Assert.Equal(pmodService, userMessageService);
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithAction()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(internalMessage);

                // Assert
                string pmodeAction = submitMessage.PMode.MessagePackaging.CollaborationInfo.Action;
                string userMessageAction = result.MessagingContext.AS4Message.FirstUserMessage.CollaborationInfo.Action;
                Assert.Equal(pmodeAction, userMessageAction);
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithSenderParty()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(internalMessage);

                // Assert
                Party pmodeParty = submitMessage.PMode.MessagePackaging.PartyInfo.FromParty;
                Party userMessageParty = result.MessagingContext.AS4Message.FirstUserMessage.Sender;
                Assert.Equal(pmodeParty, userMessageParty);
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithReceiverParty()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(internalMessage);

                // Assert
                Party pmodeParty = submitMessage.PMode.MessagePackaging.PartyInfo.ToParty;
                Party userMessageParty = result.MessagingContext.AS4Message.FirstUserMessage.Receiver;
                Assert.Equal(pmodeParty, userMessageParty);
            }

            [Fact]
            public async Task ThenStepCreatesAS4MessageWithSubmitMessageProperties()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act
                StepResult result = await ExerciseCreateAS4Message(internalMessage);

                // Assert
                AssertMessageProperty(submitMessage, result.MessagingContext);
            }

            private static void AssertMessageProperty(SubmitMessage submitMessage, MessagingContext messagingContext)
            {
                SubmitMessageProperty submitMessageProperty = submitMessage.MessageProperties.First();
                UserMessageProperty userMessageMessageProperty =
                    messagingContext.AS4Message.FirstUserMessage.MessageProperties.First();

                Assert.Equal(submitMessageProperty.Value, userMessageMessageProperty.Value);
                Assert.Equal(submitMessageProperty.Name, userMessageMessageProperty.Name);
            }
        }

        public class GivenInvalidArguments : GivenCreateAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenStepFailsToCreateAS4MessageWhenSubmitMessageTriesToOVerrideSenderPartyAsync()
            {
                // Arrange
                SubmitMessage submitMessage = SubmitWithTwoPayloads();
                submitMessage.PartyInfo = CreatePopulatedSubmitPartyInfo();
                submitMessage.PMode = DefaultSendPMode();
                var internalMessage = new MessagingContext(submitMessage);

                // Act / Assert
                await Assert.ThrowsAnyAsync<Exception>(() => ExerciseCreateAS4Message(internalMessage));
            }

            private static PartyInfo CreatePopulatedSubmitPartyInfo()
            {
                return new PartyInfo { ToParty = new AS4.Model.Common.Party(), FromParty = new AS4.Model.Common.Party() };
            }
        }

        protected SubmitMessage SubmitWithTwoPayloads()
        {
            return AS4XmlSerializer.FromString<SubmitMessage>(Properties.Resources.submitmessage);
        }

        protected SendingProcessingMode DefaultSendPMode()
        {
            return AS4XmlSerializer.FromString<SendingProcessingMode>(Properties.Resources.sendingprocessingmode);
        }

        protected async Task<StepResult> ExerciseCreateAS4Message(MessagingContext context)
        {
            var stubProvider = new Mock<IPayloadRetrieverProvider>();
            var stubRetriever = new Mock<IPayloadRetriever>();

            stubRetriever.Setup(r => r.RetrievePayloadAsync(It.IsAny<string>())).ReturnsAsync(Stream.Null);
            stubProvider.Setup(p => p.Get(It.IsAny<Payload>())).Returns(stubRetriever.Object);

            var sut = new CreateAS4MessageStep(stubProvider.Object);

            // Act
            return await sut.ExecuteAsync(context);
        }
    }
}