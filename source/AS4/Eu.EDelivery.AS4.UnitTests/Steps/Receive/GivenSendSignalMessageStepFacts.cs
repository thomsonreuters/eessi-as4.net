using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Model;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenSendSignalMessageStepFacts
    {
        [Fact]
        public async Task ReturnsEmptySoapForReceipt_IfReplyPatternIsCallback()
        {
            // Arrange
            var pmode = new ReceivingProcessingMode();
            pmode.ReplyHandling.ReplyPattern = ReplyPattern.Callback;
            MessagingContext context = ContextWithSignal(new FilledNRRReceipt(), pmode);

            // Act
            AS4Message result = await ExerciseSendSignal(context);

            // Assert
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public async Task ReturnsEmptySoapForError_IfReplyPatternIsCallback()
        {
            // Arrange
            var pmode = new ReceivingProcessingMode();
            pmode.ReplyHandling.ReplyPattern = ReplyPattern.Callback;
            MessagingContext context = ContextWithSignal(new Error(), pmode);

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

        private static async Task<AS4Message> ExerciseSendSignal(MessagingContext context)
        {
            var sut = new SendAS4SignalMessageStep();

            StepResult result = await sut.ExecuteAsync(context, default(CancellationToken));

            return result.MessagingContext.AS4Message;
        } 
    }
}
