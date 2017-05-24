using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="SendAS4ReceiptStep" />
    /// </summary>
    public class GivenSendAS4ReceiptStepFacts : GivenDatastoreFacts
    {
        private readonly SendAS4ReceiptStep _step;
        private readonly string _sharedId;

        public GivenSendAS4ReceiptStepFacts()
        {
            _sharedId = "shared-id";
            _step = new SendAS4ReceiptStep();
        }

        public class GivenValidArguments : GivenSendAS4ReceiptStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithReceiptAsync()
            {
                // Arrange
                InternalMessage internalMessage = CreateDefaultInternalMessage();

                // Act
                StepResult result = await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                SignalMessage signalMessage = result.InternalMessage.AS4Message.SignalMessages.FirstOrDefault();

                Assert.NotNull(signalMessage);
                Assert.IsType(typeof(Receipt), signalMessage);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithEmptySoapEnvelopeAsync()
            {
                // Arrange
                InternalMessage internalMessage = CreateDefaultInternalMessage();
                internalMessage.ReceivingPMode.ReceiptHandling.ReplyPattern = ReplyPattern.Callback;

                // Act
                StepResult result = await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.NotNull(result.InternalMessage.AS4Message);
                Assert.Empty(result.InternalMessage.AS4Message.UserMessages);
                Assert.Empty(result.InternalMessage.AS4Message.SignalMessages);
            }

            private InternalMessage CreateDefaultInternalMessage()
            {
                var receipt = new Receipt("message-id") {RefToMessageId = _sharedId};
                AS4Message receiptMessage = new AS4MessageBuilder().WithSignalMessage(receipt).Build();

                return new InternalMessage(receiptMessage)
                {
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };
            }
        }
    }
}