using System.Linq;
using System.Threading;
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
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="CreateAS4ErrorStep" />
    /// </summary>
    public class GivenCreateAS4ErrorStepFacts : GivenDatastoreFacts
    {
        public class GivenValidArguments : GivenCreateAS4ErrorStepFacts
        {
            public IStep Step => new CreateAS4ErrorStep();

            [Fact]
            public async Task ThenNotApplicableIfMessageIsEmptySoapBodyAsync()
            {
                // Arrange
                var fixture = new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive);

                // Act
                StepResult result = await Step.ExecuteAsync(fixture, CancellationToken.None);

                // Assert
                Assert.Equal(fixture, result.MessagingContext);
            }

            [Fact]
            public async Task ThenErrorIsCreatedWithAS4ExceptionAsync()
            {
                // Arrange
                AS4Message as4Message = CreateFilledAS4Message();
                var fixture = new MessagingContext(
                    as4Message, 
                    MessagingContextMode.Unknown)
                    {
                        ErrorResult = new ErrorResult(description: string.Empty, alias: ErrorAlias.ConnectionFailure),
                        SendingPMode = new SendingProcessingMode(),
                        ReceivingPMode = new ReceivingProcessingMode()
                    };

                // Act
                StepResult result = await Step.ExecuteAsync(fixture, CancellationToken.None);

                // Assert
                var error = result.MessagingContext.AS4Message.PrimarySignalMessage as Error;

                Assert.NotNull(error);
                Assert.Equal("message-id", error.RefToMessageId);
                Assert.Equal("EBMS:0005", error.Errors.First().ErrorCode);
            }

            [Fact]
            public async Task ThenErrorIsCreatedWithPModesAsync()
            {
                // Arrange
                var fixture = new MessagingContext(
                    CreateFilledAS4Message(), 
                    MessagingContextMode.Unknown)
                    {
                        SendingPMode = new SendingProcessingMode(),
                        ReceivingPMode = new ReceivingProcessingMode()
                    };

                // Act
                StepResult result = await Step.ExecuteAsync(fixture, CancellationToken.None);

                // Assert
                Assert.Equal(fixture.ReceivingPMode, result.MessagingContext.ReceivingPMode);
                Assert.Equal(fixture.SendingPMode, result.MessagingContext.SendingPMode);
            }

            [Fact]
            public async Task ThenErrorIsCreatedWithSigningIdAsync()
            {
                // Arrange
                AS4Message as4Message = CreateFilledAS4Message();   
                as4Message.SigningId = new SigningId("header-id", "body-id");

                var fixture = new MessagingContext(
                    as4Message, 
                    MessagingContextMode.Unknown)
                    {
                        SendingPMode = new SendingProcessingMode(),
                        ReceivingPMode = new ReceivingProcessingMode()
                    };

                // Act
                StepResult result = await Step.ExecuteAsync(fixture, CancellationToken.None);

                // Assert
                Assert.Equal(as4Message.SigningId, result.MessagingContext.AS4Message.SigningId);
            }

            private static AS4Message CreateFilledAS4Message()
            {
                return AS4Message.Create(new FilledUserMessage());
            }
        }
    }
}