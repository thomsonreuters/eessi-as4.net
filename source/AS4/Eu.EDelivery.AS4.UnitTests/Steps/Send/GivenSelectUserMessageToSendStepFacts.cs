using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SelectUserMessageToSendStep"/>
    /// </summary>
    public class GivenSelectUserMessageToSendStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task SelectionReturnsPullRequestWarning_IfNoMatchesAreFound()
        {
            // Act
            StepResult result = await ExerciseSelection(expectedMpc: null);

            // Assert
            var signal = result.MessagingContext.AS4Message.PrimarySignalMessage as PullRequestError;
            Assert.Equal(new PullRequestError(), signal);
            Assert.False(result.CanProceed);
        }

        [Fact]
        public async Task SelectsUserMessage_IfUserMessageMatchesCriteria()
        {
            // Arrange
            const string expectedMpc = "message-mpc";
            InsertUserMessage(expectedMpc, MessageExchangePattern.Push, Operation.ToBeSent);
            InsertUserMessage("yet-another-mpc", MessageExchangePattern.Pull, Operation.DeadLettered);
            InsertUserMessage(expectedMpc, MessageExchangePattern.Pull, Operation.ToBeSent);

            // Act
            StepResult result = await ExerciseSelection(expectedMpc);

            // Assert

            var as4Message = await RetrieveAS4MessageFromContext(result.MessagingContext);

            UserMessage userMessage = as4Message.PrimaryUserMessage;

            Assert.Equal(expectedMpc, userMessage.Mpc);
            AssertOutMessage(userMessage.MessageId, m => Assert.True(OperationUtils.Parse(m.Operation) == Operation.Sent));
            Assert.NotNull(result.MessagingContext.SendingPMode);
        }

        private static async Task<AS4Message> RetrieveAS4MessageFromContext(MessagingContext context)
        {
            if (context.AS4Message != null)
            {
                return context.AS4Message;
            }

            if (context.ReceivedMessage == null)
            {
                throw new InvalidOperationException("A ReceivedMessage was expected in the MessagingContext.");
            }

            var serializer = SerializerProvider.Default.Get(context.ReceivedMessage.ContentType);

            context.ReceivedMessage.UnderlyingStream.Position = 0;

            return await serializer.DeserializeAsync(context.ReceivedMessage.UnderlyingStream, context.ReceivedMessage.ContentType, CancellationToken.None);
        }

        private async Task<StepResult> ExerciseSelection(string expectedMpc)
        {
            var sut = new SelectUserMessageToSendStep(GetDataStoreContext, InMemoryMessageBodyStore.Default);
            MessagingContext context = ContextWithPullRequest(expectedMpc);

            // Act
            return await sut.ExecuteAsync(context);
        }

        private void InsertUserMessage(string mpc, MessageExchangePattern pattern, Operation operation)
        {
            MessageExchangePatternBinding GetMepBindingFromMep(MessageExchangePattern mep)
            {
                switch (mep)
                {
                    case MessageExchangePattern.Pull:
                        return MessageExchangePatternBinding.Pull;
                    default:
                        return MessageExchangePatternBinding.Push;
                }
            }

            var sendingPMode = new SendingProcessingMode()
            {
                Id = "SomePModeId",
                MepBinding = GetMepBindingFromMep(pattern)
            };

            var userMessage = UserMessageFactory.Instance.Create(sendingPMode);
            userMessage.Mpc = mpc;

            var as4Message = AS4Message.Create(userMessage, sendingPMode);

            InsertOutMessage(as4Message, operation, sendingPMode);
        }

        private void InsertOutMessage(AS4Message as4Message, Operation operation, SendingProcessingMode sendingPMode)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                var service = new OutMessageService(new DatastoreRepository(context), InMemoryMessageBodyStore.Default);

                service.InsertAS4Message(
                    new MessagingContext(as4Message, MessagingContextMode.Send)
                    {
                        SendingPMode = sendingPMode
                    },
                    operation);

                context.SaveChanges();
            }
        }

        private static MessagingContext ContextWithPullRequest(string mpc)
        {
            var pullRequest = new PullRequest(mpc, "message-id");
            return new MessagingContext(AS4Message.Create(pullRequest), MessagingContextMode.Send);
        }

        private void AssertOutMessage(string messageId, Action<OutMessage> assertion)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                OutMessage outMessage = context.OutMessages.First(m => m.EbmsMessageId.Equals(messageId));
                assertion(outMessage);
            }
        }
    }
}
