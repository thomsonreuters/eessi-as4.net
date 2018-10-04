using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="CreateAS4ErrorStep" />
    /// </summary>
    public class GivenCreateAS4ErrorStepFacts : GivenDatastoreFacts
    {
        [Property]
        public Property Creates_Error_For_Each_Bundled_UserMessage()
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

                    // Act
                    StepResult result =
                        CreateErrorStepWith("unknown-send-pmode-id")
                            .ExecuteAsync(new MessagingContext(fixture, MessagingContextMode.Receive))
                            .GetAwaiter()
                            .GetResult();

                    // Assert
                    AS4Message errorMessage = result.MessagingContext.AS4Message;
                    Assert.All(
                        errorMessage.MessageUnits,
                        messageUnit =>
                        {
                            Assert.IsType<Error>(messageUnit);
                            var error = (Error) messageUnit;
                            Assert.Contains(error.RefToMessageId, fixtureMessageIds);
                            Assert.Equal(error.RefToMessageId, error.MultiHopRouting.UnsafeGet.MessageInfo?.MessageId);
                        });
                });
        }

        [Fact]
        public async Task ThenNotApplicableIfMessageIsEmptySoapBodyAsync()
        {
            // Arrange
            var fixture = new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive);

            // Act
            StepResult result = await CreateErrorStepWith(
                referencedSendPModeId: Guid.NewGuid().ToString())
                .ExecuteAsync(fixture);

            // Assert
            Assert.Equal(fixture, result.MessagingContext);
        }

        [Fact]
        public async Task ThenErrorIsCreatedWithAS4ExceptionAsync()
        {
            // Arrange
            AS4Message as4Message = CreateFilledAS4Message();
            string sendPModeId = $"send-{Guid.NewGuid()}";
            var fixture = new MessagingContext(
                as4Message,
                MessagingContextMode.Unknown)
            {
                ErrorResult = new ErrorResult("error", ErrorAlias.ConnectionFailure),
                ReceivingPMode = CreateReceivePModeWithReferencedSendPMode(sendPModeId)
            };

            // Act
            StepResult result = await CreateErrorStepWith(sendPModeId).ExecuteAsync(fixture);

            // Assert
            var error = result.MessagingContext.AS4Message.FirstSignalMessage as Error;

            Assert.NotNull(error);
            Assert.Equal("message-id", error.RefToMessageId);
            Assert.Equal(ErrorCode.Ebms0005, error.ErrorLines.First().ErrorCode);
            Assert.Equal(sendPModeId, result.MessagingContext.SendingPMode.Id);
        }

        [Fact]
        public async Task ThenErrorIsCreatedWithPModesAsync()
        {
            // Arrange
            string sendPModeId = $"send-{Guid.NewGuid()}";
            var fixture = new MessagingContext(
                CreateFilledAS4Message(),
                MessagingContextMode.Unknown)
            {
                ReceivingPMode = CreateReceivePModeWithReferencedSendPMode(sendPModeId)
            };

            // Act
            StepResult result = await CreateErrorStepWith(sendPModeId).ExecuteAsync(fixture);

            // Assert
            Assert.Equal(fixture.ReceivingPMode, result.MessagingContext.ReceivingPMode);
            Assert.Equal(sendPModeId, result.MessagingContext.SendingPMode.Id);
        }

        [Fact]
        public async Task ThenErrorIsCreatedWithSigningIdAsync()
        {
            // Arrange
            AS4Message as4Message = CreateFilledAS4Message();
            as4Message.SigningId = new SigningId("header-id", "body-id");
            string sendPModeId = $"send-{Guid.NewGuid()}";

            var fixture = new MessagingContext(
                as4Message,
                MessagingContextMode.Unknown)
            {
                ReceivingPMode = CreateReceivePModeWithReferencedSendPMode(sendPModeId)
            };

            // Act
            StepResult result = await CreateErrorStepWith(sendPModeId).ExecuteAsync(fixture);

            // Assert
            Assert.Equal(as4Message.SigningId, result.MessagingContext.AS4Message.SigningId);
            Assert.Equal(sendPModeId, result.MessagingContext.SendingPMode.Id);
        }

        private static AS4Message CreateFilledAS4Message()
        {
            return AS4Message.Create(new FilledUserMessage());
        }

        private static ReceivingProcessingMode CreateReceivePModeWithReferencedSendPMode(string id)
        {
            return new ReceivingProcessingMode
            {
                ReplyHandling =
                {
                    SendingPMode = id
                }
            };
        }

        private IStep CreateErrorStepWith(string referencedSendPModeId)
        {
            var stub = new Mock<IConfig>();
            stub.Setup(c => c.GetSendingPMode(referencedSendPModeId))
                .Returns(new SendingProcessingMode { Id = referencedSendPModeId });

            return new CreateAS4ErrorStep(stub.Object, GetDataStoreContext);
        }
    }
}