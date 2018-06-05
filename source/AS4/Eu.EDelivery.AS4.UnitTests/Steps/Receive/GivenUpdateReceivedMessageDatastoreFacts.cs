using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenUpdateReceivedMessageDatastoreFacts : GivenDatastoreFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        [Fact]
        public async Task Updates_ToBeNotified_When_Specified_SendingPMode_And_Reference_InMessage()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertOutMessage(
                new OutMessage(ebmsMessageId),
                withReceptionAwareness: false);

            AS4Message receivedAS4Message = 
                AS4Message.Create(new Receipt { RefToMessageId = ebmsMessageId });

            // Act
            await ExerciseUpdateReceivedMessage(
                receivedAS4Message, 
                CreateNotifyAllSendingPMode(), 
                receivePMode: null);

            // Assert
            GetDataStoreContext.AssertInMessageWithRefToMessageId(
                ebmsMessageId,
                m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(Operation.ToBeNotified, OperationUtils.Parse(m.Operation));
                });

            GetDataStoreContext.AssertOutMessage(
                ebmsMessageId, 
                m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(OutStatus.Ack, OutStatusUtils.Parse(m.Status));
                });
        }

        [Fact]
        public async Task Doesnt_Update_OutMessage_If_No_MessageLocation_Can_Be_Found()
        {
            // Arrange
            string knownId = "known-id-" + Guid.NewGuid();
            GetDataStoreContext.InsertOutMessage(
                new OutMessage("unknown-id-" + Guid.NewGuid()),
                withReceptionAwareness: false);

            var ctx = new MessagingContext(
                AS4Message.Create(new Receipt { RefToMessageId = knownId }), 
                MessagingContextMode.Unknown)
                {
                    SendingPMode = CreateNotifyAllSendingPMode()
                };

            var sut = new UpdateReceivedAS4MessageBodyStep(GetDataStoreContext, _messageBodyStore);

            // Act / Assert
            await Assert.ThrowsAsync<InvalidDataException>(
                () => sut.ExecuteAsync(ctx));
        }

        [Fact]
        public async Task Updates_Status_Nack_Related_UserMessage_OutMessage()
        {
            // Arrange
            string ebmsMessageId = "error-" + Guid.NewGuid();
            GetDataStoreContext.InsertOutMessage(
                CreateOutMessage(ebmsMessageId),
                withReceptionAwareness: false);

            var error = new ErrorBuilder()
                .WithErrorResult(new ErrorResult("Some Error", ErrorAlias.ConnectionFailure))
                .WithRefToEbmsMessageId(ebmsMessageId)
                .Build();

            // Act
            await ExerciseUpdateReceivedMessage(
                AS4Message.Create(error), 
                CreateNotifyAllSendingPMode(), 
                receivePMode: null);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                ebmsMessageId, 
                m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(OutStatus.Nack, OutStatusUtils.Parse(m.Status));
                });
        }

        private static OutMessage CreateOutMessage(string messageId)
        {
            var outMessage = new OutMessage(ebmsMessageId: messageId);

            outMessage.SetStatus(OutStatus.Sent);
            outMessage.SetOperation(Operation.NotApplicable);
            outMessage.SetEbmsMessageType(MessageType.UserMessage);

            outMessage.SetPModeInformation(CreateNotifyAllSendingPMode());

            return outMessage;
        }

        private static SendingProcessingMode CreateNotifyAllSendingPMode()
        {
            return new SendingProcessingMode
            {
                Id = "receive_agent_facts_pmode",
                ReceiptHandling = { NotifyMessageProducer = true },
                ErrorHandling = { NotifyMessageProducer = true }
            };
        }

        [Theory]
        [InlineData(true, 5, "0:01:00")]
        [InlineData(false, 0, "0:00:00")]
        public async Task Updates_Error_InMessage_With_Retry_Info_When_Specified(bool enabled, int count, string interval)
        {
            // Arrange
            string ebmsMessageId = "error-" + Guid.NewGuid();
            GetDataStoreContext.InsertOutMessage(
                CreateOutMessage(ebmsMessageId),
                withReceptionAwareness: false);

            var error = new ErrorBuilder()
                .WithErrorResult(new ErrorResult("Some Error occured", ErrorAlias.ConnectionFailure))
                .WithRefToEbmsMessageId(ebmsMessageId)
                .Build();

            SendingProcessingMode pmode = CreateNotifyAllSendingPMode();
            pmode.ErrorHandling.Reliability =
                new RetryReliability
                {
                    IsEnabled = enabled,
                    RetryCount = 5,
                    RetryInterval = "0:01:00"
                };

            // Act
            await ExerciseUpdateReceivedMessage(
                AS4Message.Create(error),
                pmode,
                receivePMode: null);
            // Assert
            GetDataStoreContext.AssertInMessageWithRefToMessageId(
                ebmsMessageId,
                m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(0, m.CurrentRetryCount);
                    Assert.Equal(count, m.MaxRetryCount);
                    Assert.Equal(interval, m.RetryInterval);
                });
        }

        [Theory]
        [InlineData(true, 3, "0:00:10")]
        [InlineData(false, 0, "0:00:00")]
        public async Task Updates_Receipt_InMessage_With_Info_When_Specified(bool enabled, int count, string interval)
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertOutMessage(
                new OutMessage(ebmsMessageId),
                withReceptionAwareness: false);

            AS4Message receipt = AS4Message.Create(new Receipt { RefToMessageId = ebmsMessageId });
            SendingProcessingMode pmode = CreateNotifyAllSendingPMode();
            pmode.ReceiptHandling.Reliability = 
                new RetryReliability
                {
                    IsEnabled = enabled,
                    RetryCount = 3,
                    RetryInterval = "0:00:10"
                };

            // Act
            await ExerciseUpdateReceivedMessage(receipt, pmode, receivePMode: null);

            // Assert
            GetDataStoreContext.AssertInMessageWithRefToMessageId(
                ebmsMessageId,
                m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(0, m.CurrentRetryCount);
                    Assert.Equal(count, m.MaxRetryCount);
                    Assert.Equal(interval, m.RetryInterval);
                });

        }

        [Theory]
        [InlineData(true, 3, "0:00:05")]
        [InlineData(false, 0, "0:00:00")]
        public async Task Updates_UserMessage_InMessage_With_Retry_Info_When_Specified(bool enabled, int max, string interval)
        {
            // Arrange
            string ebmsMessageId = "user-" + Guid.NewGuid();
            AS4Message as4Message = AS4Message.Create(new FilledUserMessage(ebmsMessageId));
            var pmode = new ReceivingProcessingMode();
            pmode.MessageHandling.DeliverInformation.IsEnabled = true;
            pmode.MessageHandling.DeliverInformation.Reliability = 
                new RetryReliability
                {
                    IsEnabled = enabled,
                    RetryCount = 3,
                    RetryInterval = "0:00:05"
                };

            // Act
            await ExerciseUpdateReceivedMessage(as4Message, sendPMode: null, receivePMode: pmode);

            // Assert
            GetDataStoreContext.AssertInMessage(
                ebmsMessageId,
                m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(0, m.CurrentRetryCount);
                    Assert.Equal(max, m.MaxRetryCount);
                    Assert.Equal(interval, m.RetryInterval);
                    Assert.Equal(Operation.ToBeDelivered, OperationUtils.Parse(m.Operation));
                });
        }

        private async Task ExerciseUpdateReceivedMessage(
            AS4Message as4Message,
            SendingProcessingMode sendPMode,
            ReceivingProcessingMode receivePMode)
        {
            // We need to mimick the retrieval of the SendingPMode.
            MessagingContext ctx = CreateMessageReceivedContext(as4Message, sendPMode, receivePMode);

            var sut = new UpdateReceivedAS4MessageBodyStep(GetDataStoreContext, _messageBodyStore);
            MessagingContext savedResult = await ExecuteSaveReceivedMessage(ctx);
            await sut.ExecuteAsync(savedResult);
        }

        private static MessagingContext CreateMessageReceivedContext(
            AS4Message as4Message,
            SendingProcessingMode sendingPMode,
            ReceivingProcessingMode receivingPMode)
        {
            var stream = new MemoryStream();

            SerializerProvider
                .Default
                .Get(as4Message.ContentType)
                .Serialize(as4Message, stream, CancellationToken.None);

            stream.Position = 0;

            var receivedMessage = new ReceivedMessage(stream, as4Message.ContentType);
            var ctx = new MessagingContext(receivedMessage, MessagingContextMode.Receive)
            {
                SendingPMode = sendingPMode,
                ReceivingPMode = receivingPMode
            };
            ctx.ModifyContext(as4Message);

            return ctx;
        }

        private async Task<MessagingContext> ExecuteSaveReceivedMessage(MessagingContext context)
        {
            // The receipt needs to be saved first, since we're testing the update-step.
            var step = new SaveReceivedMessageStep(StubConfig.Default, GetDataStoreContext, _messageBodyStore);
            var result = await step.ExecuteAsync(context);

            return result.MessagingContext;
        }
    }
}