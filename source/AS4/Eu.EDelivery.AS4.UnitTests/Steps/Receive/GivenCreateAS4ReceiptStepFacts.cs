using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Xunit;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="CreateAS4ReceiptStep" />
    /// </summary>
    public class GivenCreateAS4ReceiptStepFacts
    {        
        public class GivenValidArguments : GivenCreateAS4ReceiptStepFacts
        {
            [Fact]
            public async Task ThenExecuteSucceedsWithDefaultInternalMessageAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();

                // Act
                AS4Message result = await ExerciseCreateReceipt(messagingContext);

                // Assert
                Assert.IsType<Receipt>(result.PrimarySignalMessage);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithReceiptTypeAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();

                // Act
                AS4Message result = await ExerciseCreateReceipt(messagingContext);

                // Assert
                var receiptMessage = result.PrimarySignalMessage as Receipt;
                Assert.IsType<Receipt>(receiptMessage);
                Assert.Null(receiptMessage.NonRepudiationInformation);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithSigningReceiptAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();

                // Act
                AS4Message result = await ExerciseCreateReceipt(messagingContext);

                // Assert
                Assert.False(result.IsSigned);
                var receipt = result.PrimarySignalMessage as Receipt;
                Assert.IsType<Receipt>(receipt);
                Assert.NotNull(receipt.UserMessage);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithNRRFormatAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateSignedInternalMessage();
                messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = true;

                // Act
                AS4Message result = await ExerciseCreateReceipt(messagingContext);

                // Assert
                var receiptMessage = result.PrimarySignalMessage as Receipt;
                Assert.IsType<Receipt>(receiptMessage);
                Assert.NotNull(receiptMessage.NonRepudiationInformation);
                Assert.Null(receiptMessage.UserMessage);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithSameReferenceTagsAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateSignedInternalMessage();
                messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = true;

                // Act
                AS4Message result = await ExerciseCreateReceipt(messagingContext);

                // Assert
                var receiptMessage = result.PrimarySignalMessage as Receipt;
                SecurityHeader securityHeader = messagingContext.AS4Message.SecurityHeader;
                Assert.NotNull(receiptMessage);
                Assert.NotNull(securityHeader);
                AssertSignedReferences(receiptMessage, securityHeader);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithNonNRRFormat_IfUserMessageIsNotSignedAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();
                messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = true;

                // Act
                AS4Message result = await ExerciseCreateReceipt(messagingContext);

                // Assert
                var receipt = result.PrimarySignalMessage as Receipt;
                Assert.IsType<Receipt>(receipt);
                Assert.Null(receipt.NonRepudiationInformation);
            }

            private static void AssertSignedReferences(Receipt receiptMessage, SecurityHeader securityHeader)
            {
                IEnumerable<CryptoReference> cryptoRefs = securityHeader.GetReferences();
                IEnumerable<Reference> receiptRefs =
                    receiptMessage.NonRepudiationInformation.MessagePartNRInformation.Select(i => i.Reference);

                Assert.All(cryptoRefs, cryptoRef => Assert.Contains(receiptRefs, r => r.URI.Equals(cryptoRef.Uri)));
            }
        }

        protected ReceivingProcessingMode GetReceivingPMode()
        {
            var pmode = new ReceivingProcessingMode();
            pmode.ReplyHandling.ReceiptHandling.UseNRRFormat = false;

            return pmode;
        }

        protected MessagingContext CreateDefaultInternalMessage()
        {
            return new MessagingContext(
                AS4Message.Create(new FilledUserMessage()), 
                MessagingContextMode.Receive)
                {ReceivingPMode = GetReceivingPMode()};
        }

        protected MessagingContext CreateSignedInternalMessage()
        {
            MessagingContext messagingContext = CreateDefaultInternalMessage();
            AS4Message as4Message = messagingContext.AS4Message;

            AS4MessageUtils.SignWithCertificate(as4Message, new StubCertificateRepository().GetStubCertificate());

            return messagingContext;
        }

        protected async Task<AS4Message> ExerciseCreateReceipt(MessagingContext ctx)
        {
            var sut = new CreateAS4ReceiptStep();
            StepResult result = await sut.ExecuteAsync(ctx);

            return result.MessagingContext.AS4Message;
        }
    }
}