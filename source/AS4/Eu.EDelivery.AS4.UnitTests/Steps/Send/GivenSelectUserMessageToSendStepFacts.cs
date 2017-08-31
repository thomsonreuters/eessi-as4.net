using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
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
            await InsertUserMessage(expectedMpc, MessageExchangePattern.Push, Operation.ToBeSent);
            await InsertUserMessage("yet-another-mpc", MessageExchangePattern.Pull, Operation.DeadLettered);
            await InsertUserMessage(expectedMpc, MessageExchangePattern.Pull, Operation.ToBeSent);

            // Act
            StepResult result = await ExerciseSelection(expectedMpc);

            // Assert

            var as4Message = await RetrieveAS4MessageFromContext(result.MessagingContext);

            UserMessage userMessage = as4Message.PrimaryUserMessage;

            Assert.Equal(expectedMpc, userMessage.Mpc);
            AssertOutMessage(userMessage.MessageId, m => Assert.True(m.Operation == Operation.Sent));
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
            return await sut.ExecuteAsync(context, CancellationToken.None);
        }

        private async Task InsertUserMessage(string mpc, MessageExchangePattern pattern, Operation operation)
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

            using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.as4_encrypted_envelope)))
            {
                var serializer = new SoapEnvelopeSerializer();
                AS4Message message = await serializer
                    .DeserializeAsync(messageStream, Constants.ContentTypes.Soap, CancellationToken.None);

                message.PrimaryUserMessage.Mpc = mpc;

                var sendingPMode = new SendingProcessingMode()
                {
                    Id = "SomePModeId",
                    MepBinding = GetMepBindingFromMep(pattern)
                };

                await InsertOutMessage(message, operation, sendingPMode);
            }
        }

        private async Task InsertOutMessage(AS4Message as4Message, Operation operation, SendingProcessingMode sendingPMode)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                var service = new OutMessageService(new DatastoreRepository(context), InMemoryMessageBodyStore.Default);

                await service.InsertAS4Message(
                    new MessagingContext(as4Message, MessagingContextMode.Send)
                    {
                        SendingPMode = sendingPMode
                    },
                    operation,
                    CancellationToken.None);

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
