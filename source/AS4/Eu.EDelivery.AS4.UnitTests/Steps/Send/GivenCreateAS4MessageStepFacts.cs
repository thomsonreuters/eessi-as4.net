using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using SubmitMessageProperty = Eu.EDelivery.AS4.Model.Common.MessageProperty;
using UserMessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using Service = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="CreateAS4MessageStep"/>
    /// </summary>
    public class GivenCreateAS4MessageStepFacts
    {
        public GivenCreateAS4MessageStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenCreateAS4MessageStepFacts
        {
            [Fact]
            public void ThenStepCreatesAS4Message()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
                Assert.NotNull(userMessage);
            }

            [Fact]
            public void ThenStepCreatesAS4MessageWithMessageInfo()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                MessageInfo submitMessageInfo = submitMessage.MessageInfo;
                UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
                Assert.Equal(submitMessageInfo.MessageId, userMessage.MessageId);
                Assert.Equal(submitMessageInfo.Mpc, userMessage.Mpc);
                Assert.Equal(submitMessageInfo.RefToMessageId, userMessage.RefToMessageId);
            }

            [Fact]
            public void ThenStepCreatesAS4MessageWithGeneratedMessageId()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.MessageInfo.MessageId = null;
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                Assert.NotEmpty(internalMessage.AS4Message.PrimaryUserMessage.MessageId);
            }

            [Fact]
            public void ThenStepCreatesAS4MessageWithAgreement()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertAgreementReference(submitMessage, internalMessage);
            }

            private void AssertAgreementReference(SubmitMessage submitMessage, InternalMessage internalMessage)
            {
                AgreementReference pmodeAgreementRef =
                    submitMessage.PMode.MessagePackaging.CollaborationInfo.AgreementReference;
                AgreementReference userMessageAgreementRef =
                    internalMessage.AS4Message.PrimaryUserMessage.CollaborationInfo.AgreementReference;

                Assert.Equal(pmodeAgreementRef.Value, userMessageAgreementRef.Value);
                Assert.Equal(pmodeAgreementRef.Type, userMessageAgreementRef.Type);
            }

            [Fact]
            public void ThenStepCreatesAS4MessageWithService()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                Service pmodService = submitMessage.PMode.MessagePackaging.CollaborationInfo.Service;
                Service userMessageService = internalMessage.AS4Message.PrimaryUserMessage.CollaborationInfo.Service;
                Assert.Equal(pmodService, userMessageService);
            }

            [Fact]
            public void ThenStepCreatesAS4MessageWithAction()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                string pmodeAction = submitMessage.PMode.MessagePackaging.CollaborationInfo.Action;
                string userMessageAction = internalMessage.AS4Message.PrimaryUserMessage.CollaborationInfo.Action;
                Assert.Equal(pmodeAction, userMessageAction);
            }

            [Fact]
            public void ThenStepCreatesAS4MessageWithSenderParty()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                Party pmodeParty = submitMessage.PMode.MessagePackaging.PartyInfo.FromParty;
                Party userMessageParty = internalMessage.AS4Message.PrimaryUserMessage.Sender;
                Assert.Equal(pmodeParty, userMessageParty);
            }

            [Fact]
            public void ThenStepCreatesAS4MessageWithReceiverParty()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                Party pmodeParty = submitMessage.PMode.MessagePackaging.PartyInfo.ToParty;
                Party userMessageParty = internalMessage.AS4Message.PrimaryUserMessage.Receiver;
                Assert.Equal(pmodeParty, userMessageParty);
            }

            [Fact]
            public void ThenStepCreatesAS4MessageWithSubmitMessageProperties()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                // Act
                new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertMessageProperty(submitMessage, internalMessage);
            }

            private void AssertMessageProperty(SubmitMessage submitMessage, InternalMessage internalMessage)
            {
                SubmitMessageProperty submitMessageProperty = submitMessage.MessageProperties.First();
                UserMessageProperty userMessageMessageProperty =
                    internalMessage.AS4Message.PrimaryUserMessage.MessageProperties.First();

                Assert.Equal(submitMessageProperty.Type, userMessageMessageProperty.Type);
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
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PartyInfo = CreatePopulatedSubmitPartyInfo();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                var internalMessage = new InternalMessage(submitMessage);
                
                // Act / Assert
                AS4Exception as4Exception = await Assert.ThrowsAsync<AS4Exception>(
                    () => new CreateAS4MessageStep().ExecuteAsync(internalMessage, CancellationToken.None));
                Assert.NotEmpty(as4Exception.MessageIds);
            }

            private AS4.Model.Common.PartyInfo CreatePopulatedSubmitPartyInfo()
            {
                return new AS4.Model.Common.PartyInfo
                {
                    ToParty = new AS4.Model.Common.Party(),
                    FromParty = new AS4.Model.Common.Party()
                };
            }
        }

        protected SubmitMessage CreatePopulatedSubmitMessage()
        {
            return AS4XmlSerializer.Deserialize<SubmitMessage>(Properties.Resources.submitmessage);
        }

        protected SendingProcessingMode CreatePopulatedSendingPMode()
        {
            return AS4XmlSerializer.Deserialize<SendingProcessingMode>(Properties.Resources.sendingprocessingmode);
        }
    }
}