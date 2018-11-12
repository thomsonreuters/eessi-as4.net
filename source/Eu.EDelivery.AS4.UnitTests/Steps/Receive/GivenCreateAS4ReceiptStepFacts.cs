using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.TestUtils;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenCreateAS4ReceiptStepFacts
    {
        [Property]
        public Property Creates_Receipt_For_Each_UserMessage()
        {
            return Prop.ForAll(
                Gen.Fresh(() => new UserMessage($"user-{Guid.NewGuid()}"))
                   .NonEmptyListOf()
                   .ToArbitrary(),
                userMessages =>
                {
                    // Arrange
                    AS4Message fixture = AS4Message.Create(userMessages);
                    IEnumerable<string> fixtureMessageIds = fixture.MessageIds;
                    var ctx = new MessagingContext(fixture, MessagingContextMode.Receive)
                    {
                        SendingPMode = new SendingProcessingMode()
                    };

                    // Act
                    AS4Message result = 
                        ExerciseCreateReceipt(ctx)
                            .GetAwaiter()
                            .GetResult();

                    // Assert
                    Assert.All(
                        result.MessageUnits,
                        messageUnit =>
                        {
                            Assert.IsType<Receipt>(messageUnit);
                            var receipt = (Receipt) messageUnit;
                            Assert.Contains(receipt.RefToMessageId, fixtureMessageIds);
                            Assert.Equal(receipt.RefToMessageId, receipt.UserMessage.MessageId);
                        });
                });
        }

        [Fact]
        public async Task Creates_Receipt_From_Default_UserMessage()
        {
            // Arrange
            MessagingContext messagingContext = CreateUserMessageWrappedInContext();

            // Act
            AS4Message result = await ExerciseCreateReceipt(messagingContext);

            // Assert
            Assert.IsType<Receipt>(result.FirstSignalMessage);
        }

        [Fact]
        public async Task Creates_Receipt_With_NRR_Format_For_Unsigned_UserMessage()
        {
            // Arrange
            MessagingContext messagingContext = CreateUserMessageWrappedInContext();
            messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = true;

            // Act
            AS4Message result = await ExerciseCreateReceipt(messagingContext);

            // Assert
            var receipt = result.FirstSignalMessage as Receipt;
            Assert.IsType<Receipt>(receipt);
            Assert.Null(receipt.NonRepudiationInformation);
        }

        [Fact]
        public async Task Creates_Receipt_With_NRR_Format_For_Signed_UserMessage()
        {
            // Arrange
            MessagingContext messagingContext = CreateSignedUserMessageWrappedInContext();
            messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = true;

            // Act
            AS4Message result = await ExerciseCreateReceipt(messagingContext);

            // Assert
            var receiptMessage = result.FirstSignalMessage as Receipt;
            Assert.IsType<Receipt>(receiptMessage);
            Assert.NotNull(receiptMessage.NonRepudiationInformation);
            Assert.Null(receiptMessage.UserMessage);
        }

        [Fact]
        public async Task Creates_Receipt_Without_NRR_Format_If_Specified()
        {
            // Arrange
            MessagingContext messagingContext = CreateUserMessageWrappedInContext();
            messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = false;

            // Act
            AS4Message result = await ExerciseCreateReceipt(messagingContext);

            // Assert
            var receiptMessage = result.FirstSignalMessage as Receipt;
            Assert.IsType<Receipt>(receiptMessage);
            Assert.Null(receiptMessage.NonRepudiationInformation);
        }

        [Fact]
        public async Task Creates_Receipt_With_Same_Signed_References_Tags_From_UserMessage()
        {
            // Arrange
            MessagingContext messagingContext = CreateSignedUserMessageWrappedInContext();
            messagingContext.ReceivingPMode.ReplyHandling.ReceiptHandling.UseNRRFormat = true;

            // Act
            AS4Message result = await ExerciseCreateReceipt(messagingContext);

            // Assert
            var receiptMessage = result.FirstSignalMessage as Receipt;
            SecurityHeader securityHeader = messagingContext.AS4Message.SecurityHeader;
            Assert.NotNull(receiptMessage);
            Assert.NotNull(securityHeader);
            AssertSignedReferences(receiptMessage, securityHeader);
        }

        private static MessagingContext CreateSignedUserMessageWrappedInContext()
        {
            MessagingContext messagingContext = CreateUserMessageWrappedInContext();
            AS4Message as4Message = messagingContext.AS4Message;

            AS4MessageUtils.SignWithCertificate(as4Message, new StubCertificateRepository().GetStubCertificate());

            return messagingContext;
        }

        private static void AssertSignedReferences(Receipt receiptMessage, SecurityHeader securityHeader)
        {
            IEnumerable<Reference> receiptRefs =
                receiptMessage.NonRepudiationInformation.MessagePartNRIReferences;

            Assert.All(
                securityHeader.GetReferences(), 
                cryptoRef => Assert.Contains(receiptRefs, r => r.URI.Equals(cryptoRef.Uri)));
        }

        [Fact]
        public async Task Creates_Unsigned_Receipt_From_UserMessage()
        {
            // Arrange
            MessagingContext messagingContext = CreateUserMessageWrappedInContext();

            // Act
            AS4Message result = await ExerciseCreateReceipt(messagingContext);

            // Assert
            Assert.False(result.IsSigned);
            var receipt = result.FirstSignalMessage as Receipt;
            Assert.IsType<Receipt>(receipt);
            Assert.NotNull(receipt.UserMessage);
        }

        [Fact]
        public async Task Creates_Multihop_Receipt_If_Received_UserMessage_Is_MultiHop()
        {
            // Arrange
            var fixture = new MessagingContext(
                AS4Message.Create(
                    new UserMessage($"user-{Guid.NewGuid()}"),
                    new SendingProcessingMode { MessagePackaging = { IsMultiHop = true } }),
                MessagingContextMode.Receive)
            {
                ReceivingPMode = new ReceivingProcessingMode()
            };

            // Act
            AS4Message result = await ExerciseCreateReceipt(fixture);

            // Assert
            Assert.True(result.IsMultiHopMessage);
        }

        private static MessagingContext CreateUserMessageWrappedInContext()
        {
            return new MessagingContext(
                    AS4Message.Create(new FilledUserMessage()),
                    MessagingContextMode.Receive)
            {
                ReceivingPMode = GetReceivingPMode()
            };
        }

        private static ReceivingProcessingMode GetReceivingPMode()
        {
            var pmode = new ReceivingProcessingMode();
            pmode.ReplyHandling.ReceiptHandling.UseNRRFormat = false;

            return pmode;
        }

        private static async Task<AS4Message> ExerciseCreateReceipt(MessagingContext ctx)
        {
            var sut = new CreateAS4ReceiptStep();
            StepResult result = await sut.ExecuteAsync(ctx);

            return result.MessagingContext.AS4Message;
        }
    }
}