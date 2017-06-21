using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="PullOutExceptionStepDecorator"/>
    /// </summary>
    public class GivenPullOutExceptionStepDecoratorFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task FailsToVerifySignature_ReturnsErrorMessage()
        {
            // Arrange
            PullOutExceptionStepDecorator sut = DecoratorWithPullRequestValidationExceptionSaboteur();

            // Act
            StepResult result = await sut.ExerciseStep(AnonymousContext());

            // Assert
            SignalMessage primarySignal = result.MessagingContext.AS4Message.PrimarySignalMessage;
            Assert.IsType<Error>(primarySignal);
            Assert.Equal(1, ((Error)primarySignal).Errors.Count);
            Assert.Equal(403, result.MessagingContext.ReceivingPMode.ErrorHandling.ResponseHttpCode);
        }

        private static MessagingContext AnonymousContext()
        {
            return new MessagingContext(AS4Message.Create(new SendingProcessingMode()), MessagingContextMode.Unknown);
        }

        [Fact]
        public async Task FailsToVerifySignature_ResultesInInsertedOutException()
        {
            // Arrange
            PullOutExceptionStepDecorator sut = DecoratorWithPullRequestValidationExceptionSaboteur();
            MessagingContext context = ContextWithPullRequest();

            // Act
            await sut.ExerciseStep(context);

            // Assert
            GetDataStoreContext.AssertOutException(exception =>
            {
                Assert.Null(exception.EbmsRefToMessageId);
                Assert.Equal(AS4XmlSerializer.ToSoapEnvelopeBytesAsync(context.AS4Message).Result, exception.MessageBody);
                Assert.NotEmpty(exception.Exception);
            });
        }

        private PullOutExceptionStepDecorator DecoratorWithPullRequestValidationExceptionSaboteur()
        {
            var saboteur = new SaboteurStep(PullRequestValidationException.InvalidSignature("message-id"));
            return new PullOutExceptionStepDecorator(saboteur, GetDataStoreContext);
        }

        private static MessagingContext ContextWithPullRequest()
        {
            return new MessagingContext(AS4Message.Create(new PullRequest()), MessagingContextMode.Send);
        }
    }
}
