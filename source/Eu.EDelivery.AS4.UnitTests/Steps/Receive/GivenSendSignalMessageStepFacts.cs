using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using FsCheck;
using Xunit;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenSendSignalMessageStepFacts : GivenDatastoreFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        [CustomProperty]
        public Property Returns_Empty_Soap_For_Signals_If_ReplyPattern_Is_Callback_Or_Mode_Is_PullReceive(
            SignalMessage signal,
            ReplyPattern pattern,
            MessagingContextMode mode)
        {
            // Arrange
            var context =
                new MessagingContext(AS4Message.Create(signal), mode)
                {
                    ReceivingPMode = new ReceivingProcessingMode { ReplyHandling = { ReplyPattern = pattern } },
                    SendingPMode = new SendingProcessingMode { Id = "sending-pmode" }
                };

            var sut = new SendAS4SignalMessageStep(StubConfig.Default, GetDataStoreContext, _messageBodyStore);

            // Act
            StepResult result = sut.ExecuteAsync(context).GetAwaiter().GetResult();

            // Assert
            AS4Message actual = result.MessagingContext.AS4Message;
            AS4Message expected = context.AS4Message;

            bool isCallback = pattern == ReplyPattern.Callback;
            bool isResponse = pattern == ReplyPattern.Response;
            bool isPullReceive = mode == MessagingContextMode.PullReceive;
            bool isSignal = expected.Equals(actual);

            return (actual.IsEmpty == (isCallback || isPullReceive))
                .Label("Should be an empty SOAP envelope when configured Callback or in PullReceive mode")
                .Or(isSignal == isResponse)
                .Label("Should be a SignalMessage when configured Response")
                .Classify(actual.IsEmpty, "Empty SOAP envelope response")
                .Classify(isSignal, "SignalMessage response");
        }

        [Theory]
        [InlineData(MessageExchangePattern.Pull, Operation.ToBePiggyBacked)]
        [InlineData(MessageExchangePattern.Push, Operation.ToBeSent)]
        public async Task Stores_SignalMessage_With_Expected_Operation_According_To_MEP(
            MessageExchangePattern mep,
            Operation op)
        {
            // Arrange
            string userMessageId = $"user-{Guid.NewGuid()}";
            GetDataStoreContext.InsertInMessage(
                new InMessage(userMessageId) { MEP = mep });

            var receipt = new Receipt($"receipt-{Guid.NewGuid()}", userMessageId);
            var context = new MessagingContext(
                AS4Message.Create(receipt), 
                MessagingContextMode.Receive)
                {
                    SendingPMode = new SendingProcessingMode { Id = "shortcut-send-pmode-retrieval" },
                    ReceivingPMode = new ReceivingProcessingMode
                    {
                        ReplyHandling = { ReplyPattern = ReplyPattern.Callback }
                    }
                };

            var sut = new SendAS4SignalMessageStep(StubConfig.Default, GetDataStoreContext, _messageBodyStore);

            // Act
            await sut.ExecuteAsync(context);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                receipt.MessageId,
                m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(op, m.Operation);
                });
        }
    }
}

