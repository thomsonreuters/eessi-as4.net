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
                MessagingContext messagingContext = CreateDefaultInternalMessage();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                SignalMessage signalMessage = result.MessagingContext.AS4Message.SignalMessages.FirstOrDefault();

                Assert.NotNull(signalMessage);
                Assert.IsType(typeof(Receipt), signalMessage);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithEmptySoapEnvelopeAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();
                messagingContext.ReceivingPMode.ReceiptHandling.ReplyPattern = ReplyPattern.Callback;

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                Assert.NotNull(result.MessagingContext.AS4Message);
                Assert.Empty(result.MessagingContext.AS4Message.UserMessages);
                Assert.Empty(result.MessagingContext.AS4Message.SignalMessages);
            }

            private MessagingContext CreateDefaultInternalMessage()
            {
                var receipt = new Receipt("message-id") {RefToMessageId = _sharedId};
                AS4Message receiptMessage = AS4Message.Create(receipt);

                return new MessagingContext(receiptMessage)
                {
                    SendingPMode = new SendingProcessingMode(),
                    ReceivingPMode = new ReceivingProcessingMode()
                };
            }
        }
    }
}