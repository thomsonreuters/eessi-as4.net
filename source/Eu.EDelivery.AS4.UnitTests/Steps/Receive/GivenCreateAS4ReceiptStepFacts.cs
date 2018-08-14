using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
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
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenCreateAS4ReceiptStepFacts
    {
        private const string NonMultihopSendPModeId = "non-multihop-send-id";

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
        public async Task Creates_Multihop_Receipt_If_Specified_In_Response_PMode()
        {
            // Arrange
            string sendPModeId = $"send-id-{Guid.NewGuid()}";
            MessagingContext fixture = CreateUserMessageWrappedInContext(sendPModeId);

            var stub = new StubConfig(
                sendingPModes: new Dictionary<string, SendingProcessingMode>
                {
                    [sendPModeId] = new SendingProcessingMode
                    {
                        Id = sendPModeId,
                        MessagePackaging = { IsMultiHop = true }
                    }
                },
                receivingPModes: new Dictionary<string, ReceivingProcessingMode>());

            // Act
            MessagingContext result = await ExerciseCreateReceipt(fixture, stub);

            // Assert
            Assert.True(result.AS4Message.IsMultiHopMessage);
            Assert.Equal(sendPModeId, result.SendingPMode.Id);
        }

        private static MessagingContext CreateUserMessageWrappedInContext(
            string responsePModeId = NonMultihopSendPModeId)
        {
            return new MessagingContext(
                    AS4Message.Create(new FilledUserMessage()),
                    MessagingContextMode.Receive)
            {
                ReceivingPMode = GetReceivingPMode(responsePModeId)
            };
        }

        private static ReceivingProcessingMode GetReceivingPMode(string responsePModeId)
        {
            var pmode = new ReceivingProcessingMode();
            pmode.ReplyHandling.ReceiptHandling.UseNRRFormat = false;
            pmode.ReplyHandling.SendingPMode = responsePModeId;

            return pmode;
        }

        private static async Task<AS4Message> ExerciseCreateReceipt(MessagingContext ctx)
        {
            var stub = new Mock<IConfig>();
            stub.Setup(c => c.GetSendingPMode(It.IsAny<string>()))
                .Returns(new SendingProcessingMode());

            MessagingContext result = await ExerciseCreateReceipt(ctx, stub.Object);
            return result.AS4Message;
        }

        private static async Task<MessagingContext> ExerciseCreateReceipt(
            MessagingContext ctx,
            IConfig stub)
        {
            var sut = new CreateAS4ReceiptStep(stub);
            StepResult result = await sut.ExecuteAsync(ctx);

            return result.MessagingContext;
        }
    }
}