using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
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
                        CreateErrorStep()
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
        public async Task Skips_Create_Error_When_AS4Message_Is_Empty()
        {
            // Arrange
            var fixture = new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive);

            // Act
            StepResult result = await CreateErrorStep()
                .ExecuteAsync(fixture);

            // Assert
            Assert.Equal(fixture, result.MessagingContext);
        }

        [Fact]
        public async Task Creates_Error_Based_On_ErrorResult_Information()
        {
            // Arrange
            AS4Message as4Message = CreateFilledAS4Message();
            var fixture = new MessagingContext(
                as4Message,
                MessagingContextMode.Unknown)
            {
                ErrorResult = new ErrorResult("error", ErrorAlias.ConnectionFailure),
                ReceivingPMode = new ReceivingProcessingMode()
            };

            // Act
            StepResult result = await CreateErrorStep().ExecuteAsync(fixture);

            // Assert
            var error = result.MessagingContext.AS4Message.FirstSignalMessage as Error;

            Assert.NotNull(error);
            Assert.Equal("message-id", error.RefToMessageId);
            Assert.Equal(ErrorCode.Ebms0005, error.ErrorLines.First().ErrorCode);
        }

        [Fact]
        public async Task Creates_Error_With_Same_SigningId_As_Received_UserMessage()
        {
            // Arrange
            AS4Message as4Message = CreateFilledAS4Message();
            as4Message.SigningId = new SigningId("header-id", "body-id");

            var fixture = new MessagingContext(
                as4Message,
                MessagingContextMode.Unknown)
            {
                ReceivingPMode = new ReceivingProcessingMode()
            };

            // Act
            StepResult result = await CreateErrorStep().ExecuteAsync(fixture);

            // Assert
            Assert.Equal(as4Message.SigningId, result.MessagingContext.AS4Message.SigningId);
        }

        [Fact]
        public async Task Creates_MultiHop_Error_If_Received_UserMessage_Is_MultiHop()
        {
            // Arrange
            var ctx = new MessagingContext(
                AS4Message.Create(
                    new UserMessage($"user-{Guid.NewGuid()}"),
                    new SendingProcessingMode { MessagePackaging = { IsMultiHop = true } }),
                MessagingContextMode.Receive)
            {
                ReceivingPMode = new ReceivingProcessingMode()
            };

            // Act
            AS4Message actual = await ExerciseCreateError(ctx);

            // Assert
            Assert.IsType<Error>(actual.PrimaryMessageUnit);
            Assert.True(actual.IsMultiHopMessage, "Is not multi-hop message");
        }

        private static AS4Message CreateFilledAS4Message()
        {
            return AS4Message.Create(new FilledUserMessage());
        }

        private IStep CreateErrorStep()
        {
            return new CreateAS4ErrorStep(GetDataStoreContext);
        }

        private async Task<AS4Message> ExerciseCreateError(MessagingContext ctx)
        {
            var sut = new CreateAS4ErrorStep(GetDataStoreContext);
            StepResult result = await sut.ExecuteAsync(ctx);

            return result.MessagingContext.AS4Message;
        }
    }
}