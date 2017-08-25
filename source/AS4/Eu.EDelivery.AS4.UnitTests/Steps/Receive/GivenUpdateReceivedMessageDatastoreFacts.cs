using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenUpdateReceivedMessageDatastoreFacts : GivenDatastoreStepFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenUpdateReceivedMessageDatastoreFacts" /> class.
        /// </summary>
        public GivenUpdateReceivedMessageDatastoreFacts()
        {
            Step = new UpdateReceivedAS4MessageBodyStep(GetDataStoreContext, _messageBodyStore);
        }

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        public class GivenReceivedReceiptMessage : GivenUpdateReceivedMessageDatastoreFacts
        {
            private const string EbmsMessageId = "some-messageid";

            [Fact]
            public async Task ThenOperationIsToBeNotified()
            {
                // Arrange
                InsertOutMessage();

                var receivedAS4Message = AS4Message.Create(new Receipt { RefToMessageId = EbmsMessageId });

                MessagingContext context = CreateMessageReceivedContext(receivedAS4Message, null);
                // We need to mimick the retrieval of the SendingPMode.
                context.SendingPMode = GetSendingPMode();

                context = await ExecuteSaveReceivedMessage(context);

                // Act
                await Step.ExecuteAsync(context, CancellationToken.None);

                // Assert
                InMessage inMessage = GetInMessageWithRefToMessageId(EbmsMessageId);
                Assert.NotNull(inMessage);
                Assert.Equal(Operation.ToBeNotified, inMessage.Operation);

                OutMessage outMessage = GetOutMessage(EbmsMessageId);
                Assert.NotNull(outMessage);
                Assert.Equal(OutStatus.Ack, outMessage.Status);
            }

            private static MessagingContext ReceiptAS4MessageWithSendingPMode(string refToMessageId)
            {
                var receipt = new Receipt { RefToMessageId = refToMessageId };

                AS4Message as4Message = AS4Message.Create(receipt);

                return new MessagingContext(as4Message, MessagingContextMode.Unknown) { SendingPMode = GetSendingPMode() };
            }

            [Fact]
            public async Task DoesntUpdateMessages_IfNoMessageLocationCanBeFound()
            {
                // Arrange
                InsertOutMessage("other message id");
                MessagingContext message = ReceiptAS4MessageWithSendingPMode(EbmsMessageId);

                // Act / Assert
                await Assert.ThrowsAsync<InvalidDataException>(
                    () => Step.ExecuteAsync(message, CancellationToken.None));
            }

            private void InsertOutMessage(string messageId = EbmsMessageId)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    context.OutMessages.Add(new OutMessage { EbmsMessageId = messageId });
                    context.SaveChanges();
                }
            }
        }

        public class GivenReceivedErrorMessage : GivenUpdateReceivedMessageDatastoreFacts
        {
            private const string EbmsMessageId = "some-messageid";

            [Fact]
            public async Task ThenRelatedUserMessageStatusIsSetToNAck()
            {
                // Arrange
                await InsertOutMessageWith(EbmsMessageId);

                var error = new ErrorBuilder().WithErrorResult(new ErrorResult("Some Error", ErrorAlias.ConnectionFailure))
                                              .WithRefToEbmsMessageId(EbmsMessageId)
                                              .Build();

                var receivedError = AS4Message.Create(error);

                var context = CreateMessageReceivedContext(receivedError, null);
                context.SendingPMode = GetSendingPMode();

                context = await ExecuteSaveReceivedMessage(context);

                // Act
                await Step.ExecuteAsync(context, CancellationToken.None);

                // Assert
                OutMessage outMessage = GetOutMessage(EbmsMessageId);
                Assert.NotNull(outMessage);
                Assert.Equal(OutStatus.Nack, outMessage.Status);
            }

            private async Task InsertOutMessageWith(string messageId)
            {
                using (DatastoreContext db = GetDataStoreContext())
                {
                    db.OutMessages.Add(CreateOutMessage(messageId));
                    await db.SaveChangesAsync();
                }
            }
        }

        private async Task<MessagingContext> ExecuteSaveReceivedMessage(MessagingContext context)
        {
            // The receipt needs to be saved first, since we're testing the update-step.
            var step = new SaveReceivedMessageStep(GetDataStoreContext, _messageBodyStore);
            var result = await step.ExecuteAsync(context, CancellationToken.None);

            return result.MessagingContext;
        }

        private static OutMessage CreateOutMessage(string messageId)
        {
            var outMessage = new OutMessage
            {
                EbmsMessageId = messageId,
                Status = OutStatus.Sent,
                Operation = Operation.NotApplicable,
                EbmsMessageType = MessageType.UserMessage,
            };

            outMessage.SetPModeInformation(GetSendingPMode());

            return outMessage;
        }

        protected MessagingContext CreateMessageReceivedContext(AS4Message as4Message, ReceivingProcessingMode receivingPMode)
        {
            MemoryStream stream = new MemoryStream();

            SerializerProvider.Default.Get(as4Message.ContentType).Serialize(as4Message, stream, CancellationToken.None);
            stream.Position = 0;

            var receivedMessage = new ReceivedMessage(stream, as4Message.ContentType);

            var messagingContext = new MessagingContext(receivedMessage, MessagingContextMode.Receive) { ReceivingPMode = receivingPMode };

            return messagingContext;
        }

        private static SendingProcessingMode GetSendingPMode()
        {
            return new SendingProcessingMode
            {
                Id = "receive_agent_facts_pmode",
                ReceiptHandling = { NotifyMessageProducer = true },
                ErrorHandling = { NotifyMessageProducer = true }
            };
        }

        private InMessage GetInMessageWithRefToMessageId(string refToMessageId)
        {
            using (DatastoreContext db = GetDataStoreContext())
            {
                return db.InMessages.FirstOrDefault(m => m.EbmsRefToMessageId == refToMessageId);
            }
        }

        private OutMessage GetOutMessage(string messageId)
        {
            using (DatastoreContext db = GetDataStoreContext())
            {
                return db.OutMessages.FirstOrDefault(m => m.EbmsMessageId == messageId);
            }
        }
    }
}