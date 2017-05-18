using System;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
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
        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenUpdateReceivedMessageDatastoreFacts"/> class.
        /// </summary>
        public GivenUpdateReceivedMessageDatastoreFacts()
        {
            Step = new UpdateReceivedAS4MessageBodyStep(GetDataStoreContext, StubMessageBodyPersister.Default);
        }

        public class GivenReceivedReceiptMessage : GivenUpdateReceivedMessageDatastoreFacts
        {
            private const string EbmsMessageId = "some-messageid";

            /// <summary>
            /// Initializes a new instance of the <see cref="GivenUpdateReceivedMessageDatastoreFacts.GivenReceivedReceiptMessage"/> class.
            /// </summary>
            public GivenReceivedReceiptMessage()
            {
                using (var db = GetDataStoreContext())
                {
                    db.OutMessages.Add(CreateOutMessage(EbmsMessageId));
                    db.SaveChanges();
                }
            }

            [Fact]
            public async void ThenRelatedUserMessageStatusIsSetToAck()
            {
                // Arrange
                // The receipt needs to be saved first, since we're testing the update-step.
                var message = new InternalMessage(CreateReceiptAS4Message(EbmsMessageId));
                var step = new SaveReceivedMessageStep(CreateDataContext(), StubMessageBodyPersister.Default);
                await step.ExecuteAsync(message, CancellationToken.None);

                // Act
                await Step.ExecuteAsync(message, CancellationToken.None);

                // Assert
                var outMessage = GetOutMessage(EbmsMessageId);
                Assert.NotNull(outMessage);
                Assert.Equal(OutStatus.Ack, outMessage.Status);
            }

            [Fact]
            public async void ThenOperationIsToBeNotified()
            {
                // Arrange
                // The receipt needs to be saved first, since we're testing the update-step.
                var message = new InternalMessage(CreateReceiptAS4Message(EbmsMessageId));
                var step = new SaveReceivedMessageStep(CreateDataContext(), StubMessageBodyPersister.Default);
                await step.ExecuteAsync(message, CancellationToken.None);

                // Act
                await Step.ExecuteAsync(message, CancellationToken.None);

                // Assert
                var inMessage = GetInMessageWithRefToMessageId(EbmsMessageId);
                Assert.NotNull(inMessage);
                Assert.Equal(Operation.ToBeNotified, inMessage.Operation);
            }

            private static AS4Message CreateReceiptAS4Message(string refToMessageId)
            {
                var receipt = new Receipt() { RefToMessageId = refToMessageId };

                return new AS4MessageBuilder().WithSignalMessage(receipt).WithSendingPMode(GetSendingPMode()).Build();
            }
        }

        public class GivenReceivedErrorMessage : GivenUpdateReceivedMessageDatastoreFacts
        {
            private const string EbmsMessageId = "some-messageid";

            public GivenReceivedErrorMessage()
            {
                using (var db = GetDataStoreContext())
                {
                    db.OutMessages.Add(CreateOutMessage(EbmsMessageId));
                    db.SaveChanges();
                }
            }

            [Fact]
            public async void ThenRelatedUserMessageStatusIsSetToNAck()
            {
                // Arrange
                // The receipt needs to be saved first, since we're testing the update-step.
                var message = new InternalMessage(CreateErrorAS4Message(EbmsMessageId));
                var step = new SaveReceivedMessageStep(CreateDataContext(), StubMessageBodyPersister.Default);
                await step.ExecuteAsync(message, CancellationToken.None);

                // Act
                await Step.ExecuteAsync(message, CancellationToken.None);

                // Assert
                var outMessage = GetOutMessage(EbmsMessageId);
                Assert.NotNull(outMessage);
                Assert.Equal(OutStatus.Ack, outMessage.Status);
            }

            private static AS4Message CreateErrorAS4Message(string refToMessageId)
            {
                var receipt = new Receipt() { RefToMessageId = refToMessageId };

                return new AS4MessageBuilder().WithSignalMessage(receipt).WithSendingPMode(GetSendingPMode()).Build();
            }
        }

        private Func<DatastoreContext> CreateDataContext()
        {
            return () => new DatastoreContext(Options);
        }

        private static OutMessage CreateOutMessage(string messageId)
        {
            OutMessage message = new OutMessage();

            message.EbmsMessageId = messageId;
            message.Status = OutStatus.Sent;
            message.Operation = Operation.NotApplicable;
            message.EbmsMessageType = MessageType.UserMessage;
            message.PMode = AS4XmlSerializer.ToString(GetSendingPMode());

            return message;
        }

        private static SendingProcessingMode GetSendingPMode()
        {
            var pmode = new SendingProcessingMode();

            pmode.Id = "receive_agent_facts_pmode";

            pmode.ReceiptHandling.NotifyMessageProducer = true;

            pmode.ErrorHandling.NotifyMessageProducer = true;

            return pmode;
        }

        private InMessage GetInMessageWithRefToMessageId(string refToMessageId)
        {
            using (var db = GetDataStoreContext())
            {
                return db.InMessages.FirstOrDefault(m => m.EbmsRefToMessageId == refToMessageId);
            }
        }

        private OutMessage GetOutMessage(string messageId)
        {
            using (var db = GetDataStoreContext())
            {
                return db.OutMessages.FirstOrDefault(m => m.EbmsMessageId == messageId);
            }
        }
    }
}