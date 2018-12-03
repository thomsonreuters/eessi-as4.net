using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Encryption;
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
                        ExerciseCreateReceiptAsync(ctx)
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
            AS4Message result = await ExerciseCreateReceiptAsync(messagingContext);

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
            AS4Message result = await ExerciseCreateReceiptAsync(messagingContext);

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
            AS4Message result = await ExerciseCreateReceiptAsync(messagingContext);

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
            AS4Message result = await ExerciseCreateReceiptAsync(messagingContext);

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
            AS4Message result = await ExerciseCreateReceiptAsync(messagingContext);

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
            AS4Message result = await ExerciseCreateReceiptAsync(messagingContext);

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
                AS4Message.Create(new UserMessage($"user-{Guid.NewGuid()}")),
                MessagingContextMode.Receive)
                {
                    ReceivingPMode = new ReceivingProcessingMode()
                };

            // Act
            AS4Message result = await ExerciseCreateReceiptAsync(fixture);

            // Assert
            Assert.True(result.IsMultiHopMessage);
        }

        [Fact]
        public async Task Fails_To_Create_NonRepudiation_Unsigned_Receipt()
        {
            // Arrange
            var as4Message = AS4Message.Create(new UserMessage($"user-{Guid.NewGuid()}"));
            var fixture = new MessagingContext(
                as4Message, MessagingContextMode.Receive)
                {
                    ReceivingPMode = new ReceivingProcessingMode
                    {
                        ReplyHandling =
                        {
                            ReceiptHandling = { UseNRRFormat = true },
                            ResponseSigning = { IsEnabled = false }
                        }
                    }
                };

            var sut = new CreateAS4ReceiptStep();

            // Act
            StepResult result = await sut.ExecuteAsync(fixture);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.MessagingContext.ErrorResult);
        }

        [Fact]
        public async Task Fallback_To_Regular_Receipt_When_Referenced_UserMessage_Isnt_Signed()
        {
            // Arrange
            var as4Message = AS4Message.Create(
                new UserMessage($"user-{Guid.NewGuid()}"));

            var certRepo = new CertificateRepository(StubConfig.Default);

            as4Message.Encrypt(
                new KeyEncryptionConfiguration(
                    certRepo.GetCertificate(X509FindType.FindBySubjectName, "AccessPointB")), 
                DataEncryptionConfiguration.Default);

            var fixture = new MessagingContext(as4Message, MessagingContextMode.Receive)
            {
                ReceivingPMode = new ReceivingProcessingMode()
            };

            // Act
            AS4Message result = await ExerciseCreateReceiptAsync(fixture);

            // Assert
            var receipt = Assert.IsType<Receipt>(result.FirstSignalMessage);
            Assert.Null(receipt.NonRepudiationInformation);
            Assert.NotNull(receipt.UserMessage);
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

        private static async Task<AS4Message> ExerciseCreateReceiptAsync(MessagingContext ctx)
        {
            var sut = new CreateAS4ReceiptStep();
            StepResult result = await sut.ExecuteAsync(ctx);

            return result.MessagingContext.AS4Message;
        }
    }
}