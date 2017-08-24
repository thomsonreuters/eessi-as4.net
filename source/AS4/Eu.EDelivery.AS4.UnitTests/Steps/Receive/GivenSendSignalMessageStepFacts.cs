using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenSendSignalMessageStepFacts : GivenDatastoreStepFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenSendSignalMessageStepFacts"/> class.
        /// </summary>
        public GivenSendSignalMessageStepFacts()
        {
            Step = new SendAS4SignalMessageStep(GetDataStoreContext, _messageBodyStore);
        }

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        [Fact]
        public async Task ReturnsEmptySoapForReceipt_IfReplyPatternIsCallback()
        {
            // Arrange
            var receivePMode = new ReceivingProcessingMode();
            receivePMode.ReplyHandling.ReplyPattern = ReplyPattern.Callback;

            var sendPMode = new SendingProcessingMode();
            sendPMode.Id = "sending-pmode";

            MessagingContext context = ContextWithSignal(new FilledNRRReceipt(), receivePMode);
            context.SendingPMode = sendPMode;

            // Act
            AS4Message result = await ExerciseSendSignal(context);

            // Assert
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public async Task ReturnsEmptySoapForError_IfReplyPatternIsCallback()
        {
            // Arrange
            var receivePMode = new ReceivingProcessingMode();
            receivePMode.ReplyHandling.ReplyPattern = ReplyPattern.Callback;
            receivePMode.ReplyHandling.SendingPMode = "sending-pmode";
            var sendPMode = new SendingProcessingMode();
            sendPMode.Id = "sending-pmode";

            MessagingContext context = ContextWithSignal(new Error(), receivePMode);
            context.SendingPMode = sendPMode;

            // Act
            AS4Message result = await ExerciseSendSignal(context);

            // Assert
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public async Task ReturnsSameResultForReceipt_IfReplyPatternIsResponse()
        {
            // Arrange
            var pmode = new ReceivingProcessingMode();
            pmode.ReplyHandling.ReplyPattern = ReplyPattern.Response;
            MessagingContext context = ContextWithSignal(new FilledNRRReceipt(), pmode);

            // Act
            AS4Message actual = await ExerciseSendSignal(context);

            // Assert
            AS4Message expected = context.AS4Message;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ReturnsSameResultForError_IfReplyPatternIsResponse()
        {
            // Arrange
            var pmode = new ReceivingProcessingMode();
            pmode.ReplyHandling.ReplyPattern = ReplyPattern.Response;
            MessagingContext context = ContextWithSignal(new FilledNRRReceipt(), pmode);

            // Act
            AS4Message actual = await ExerciseSendSignal(context);

            // Assert
            AS4Message expected = context.AS4Message;
            Assert.Equal(expected, actual);
        }

        private static MessagingContext ContextWithSignal(SignalMessage signal, ReceivingProcessingMode pmode)
        {
            return new MessagingContext(AS4Message.Create(signal), MessagingContextMode.Receive)
            {
                ReceivingPMode = pmode
            };
        }

        private async Task<AS4Message> ExerciseSendSignal(MessagingContext context)
        {
            StepResult result = await Step.ExecuteAsync(context, default(CancellationToken));

            return result.MessagingContext.AS4Message;
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }
    }
}
