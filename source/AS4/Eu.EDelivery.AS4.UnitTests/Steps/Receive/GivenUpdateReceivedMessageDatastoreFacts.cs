using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;
using RetryReliability = Eu.EDelivery.AS4.Model.PMode.RetryReliability;

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
                AS4Message.Create(new Receipt(ebmsMessageId));

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
                    Assert.Equal(Operation.ToBeNotified, m.Operation);
                });

            GetDataStoreContext.AssertOutMessage(
                ebmsMessageId,
                m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(OutStatus.Ack, m.Status.ToEnum<OutStatus>());
                });
        }

        [Fact]
        public async Task Doesnt_Update_OutMessage_If_No_MessageLocation_Can_Be_Found()
        {
            // Arrange
            string knownId = "known-id-" + Guid.NewGuid();
            GetDataStoreContext.InsertOutMessage(
                new OutMessage(knownId) {MessageLocation = null},
                withReceptionAwareness: false);

            var ctx = new MessagingContext(
                AS4Message.Create(new FilledUserMessage(knownId)),
                MessagingContextMode.Unknown)
            {
                SendingPMode = CreateNotifyAllSendingPMode()
            };

            var sut = new UpdateReceivedAS4MessageBodyStep(StubConfig.Default, GetDataStoreContext, _messageBodyStore);

            // Act
            await sut.ExecuteAsync(ctx);

            // Assert
            GetDataStoreContext.AssertOutMessage(knownId, m => Assert.Null(m.MessageLocation));
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
                    Assert.Equal(OutStatus.Nack, m.Status.ToEnum<OutStatus>());
                });
        }

        private static OutMessage CreateOutMessage(string messageId)
        {
            var outMessage = new OutMessage(ebmsMessageId: messageId);

            outMessage.SetStatus(OutStatus.Sent);
            outMessage.Operation = Operation.NotApplicable;
            outMessage.EbmsMessageType = MessageType.UserMessage;

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
        [InlineData(true, 5, "0:00:01:00")]
        [InlineData(false, 0, "0:00:00:00")]
        public async Task Updates_Error_InMessage_With_Retry_Info_When_Specified(bool enabled, int count, string intervalString)
        {
            TimeSpan interval = intervalString.AsTimeSpan();

            // Arrange
            string ebmsMessageId = "error-" + Guid.NewGuid();
            OutMessage om = GetDataStoreContext.InsertOutMessage(
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
                    RetryInterval = "0:00:01:00"
                };

            // Act
            await ExerciseUpdateReceivedMessage(
                AS4Message.Create(error),
                pmode,
                receivePMode: null);
            // Assert
            GetDataStoreContext.AssertRetryRelatedInMessage(
                om.Id,
                rr =>
                {
                    Assert.True(enabled == (rr != null), "RetryReliability inserted while not enabled");
                    Assert.True(enabled == (0 == rr?.CurrentRetryCount), "CurrentRetryCount != 0 when enabled");
                    Assert.True(enabled == (count == rr?.MaxRetryCount), $"MaxRetryCount {count} != {rr?.MaxRetryCount} when enabled");
                    Assert.True(
                        enabled == (interval == rr?.RetryInterval),
                        $"RetryInterval {interval} != {rr?.RetryInterval} when enabled");
                });
        }

        [Theory]
        [InlineData(true, 3, "0:00:00:10")]
        [InlineData(false, 0, "0:00:00")]
        public async Task Updates_Receipt_InMessage_With_Info_When_Specified(bool enabled, int count, string intervalString)
        {
            TimeSpan interval = intervalString.AsTimeSpan();

            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertOutMessage(
                new OutMessage(ebmsMessageId),
                withReceptionAwareness: false);

            AS4Message receipt = AS4Message.Create(new Receipt(ebmsMessageId));
            SendingProcessingMode pmode = CreateNotifyAllSendingPMode();
            pmode.ReceiptHandling.Reliability =
                new RetryReliability
                {
                    IsEnabled = enabled,
                    RetryCount = 3,
                    RetryInterval = "0:00:00:10"
                };

            // Act
            await ExerciseUpdateReceivedMessage(receipt, pmode, receivePMode: null);

            // Assert
            long id = GetDataStoreContext.GetInMessage(m => m.EbmsRefToMessageId == ebmsMessageId).Id;
            GetDataStoreContext.AssertRetryRelatedInMessage(
                id,
                rr =>
                {
                    Assert.True(enabled == (rr != null), "RetryReliability inserted while not enabled");
                    Assert.True(enabled == (0 == rr?.CurrentRetryCount), "CurrentRetryCount != 0 when enabled");
                    Assert.True(enabled == (count == rr?.MaxRetryCount), $"MaxRetryCount {count} != {rr?.MaxRetryCount} when enabled");
                    Assert.True(
                        enabled == (interval == rr?.RetryInterval),
                        $"RetryInterval {interval} != {rr?.RetryInterval} when enabled");
                });

        }

        [Theory]
        [InlineData(false, false, 0, "0:00:00")]
        [InlineData(false, true, 0, "0:00:00")]
        [InlineData(true, false, 3, "0:00:00:05")]
        [InlineData(true, true, 0, "0:00:00")]
        public async Task Updates_UserMessage_InMessage_With_Retry_Info_When_Specified(
            bool enabled,
            bool isTest,
            int count,
            string intervalString)
        {
            TimeSpan interval = intervalString.AsTimeSpan();

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
                    RetryInterval = "0:00:00:05"
                };

            // Act
            await ExerciseUpdateReceivedMessage(
                as4Message,
                sendPMode: null,
                receivePMode: pmode,
                alterAfterSaved: as4 => as4.FirstUserMessage.IsTest = isTest);

            // Assert
            InMessage actual = GetDataStoreContext.GetInMessage(m => m.EbmsMessageId == ebmsMessageId);
            bool needsToBeDelivered = enabled && !isTest;
            Assert.True(
                !isTest == (Operation.ToBeDelivered == actual.Operation),
                "InMessage.Operation <> ToBeDelivered when not test message");

            GetDataStoreContext.AssertRetryRelatedInMessage(
                actual.Id,
                rr =>
                {
                    Assert.True(
                        needsToBeDelivered == (rr != null),
                        "RetryReliability inserted while not enabled and not test message");

                    Assert.True(
                        needsToBeDelivered == (0 == rr?.CurrentRetryCount),
                        "CurrentRetryCount != 0 when enabled and not test message");

                    Assert.True(
                        needsToBeDelivered == (count == rr?.MaxRetryCount),
                        $"MaxRetryCount {count} != {rr?.MaxRetryCount} when enabled and not test message");

                    Assert.True(
                        needsToBeDelivered == (interval == rr?.RetryInterval),
                        $"RetryInterval {interval} != {rr?.RetryInterval} when enabled");

                });
        }

        private async Task ExerciseUpdateReceivedMessage(
            AS4Message as4Message,
            SendingProcessingMode sendPMode,
            ReceivingProcessingMode receivePMode,
            Action<AS4Message> alterAfterSaved = null)
        {
            // We need to mimick the retrieval of the SendingPMode.
            MessagingContext ctx = CreateMessageReceivedContext(as4Message, sendPMode, receivePMode);

            var sut = new UpdateReceivedAS4MessageBodyStep(StubConfig.Default, GetDataStoreContext, _messageBodyStore);
            MessagingContext savedResult = await ExecuteSaveReceivedMessage(ctx);
            alterAfterSaved?.Invoke(savedResult.AS4Message);

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