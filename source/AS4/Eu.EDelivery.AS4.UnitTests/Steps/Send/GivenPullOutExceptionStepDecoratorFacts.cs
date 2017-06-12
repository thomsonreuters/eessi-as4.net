using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
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
            PullOutExceptionStepDecorator sut = DecoratorWitSInvalidSignatureSabotuer();

            // Act
            StepResult result = await sut.ExerciseStep(AnonymousContext());

            // Assert
            SignalMessage primarySignal = result.MessagingContext.AS4Message.PrimarySignalMessage;
            Assert.IsType<Error>(primarySignal);
            Assert.Equal(1, ((Error)primarySignal).Errors.Count);
        }

        private static MessagingContext AnonymousContext()
        {
            return new MessagingContext(AS4Message.ForSendingPMode(new SendingProcessingMode()));
        }

        [Fact]
        public async Task FailsToVerifySignature_ResultesInInsertedOutException()
        {
            // Arrange
            PullOutExceptionStepDecorator sut = DecoratorWitSInvalidSignatureSabotuer();
            MessagingContext context = ContextWithPullRequest();

            // Act
            await sut.ExerciseStep(context);

            // Assert
            AssertOnOutException(context.AS4Message);
        }

        private PullOutExceptionStepDecorator DecoratorWitSInvalidSignatureSabotuer()
        {
            var saboteur = new SaboteurStep(PullException.InvalidSignature("message-id"));
            return new PullOutExceptionStepDecorator(saboteur, GetDataStoreContext);
        }

        private static MessagingContext ContextWithPullRequest()
        {
            AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(new PullRequest()).Build();
            return new MessagingContext(as4Message);
        }

        private void AssertOnOutException(AS4Message as4Message)
        {
            OutException exception = GetDataStoreContext().Using(c => c.OutExceptions.First());

            Assert.Null(exception.EbmsRefToMessageId);
            Assert.Equal(as4Message.AsBytes(), exception.MessageBody);
            Assert.NotEmpty(exception.Exception);
        }
    }
}
