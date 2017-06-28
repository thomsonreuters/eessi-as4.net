using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Services;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    public class GivenVerifyPullRequestAuthorizationStepFacts
    {
        [Fact]
        public async Task ContinuesExecution_IfMatchedCertificateCanBeFoundForTheMpc()
        {
            // Arrange
            const string expectedMpc = "message-mpc";
            MessagingContext context = ContextWithPullRequest(expectedMpc);

            var stubMap = new StubAuthorizationMap((r, c) => r.Mpc.Equals(expectedMpc));
            var sut = new VerifyPullRequestAuthorizationStep(stubMap);

            // Act
            StepResult result = await sut.ExecuteAsync(context, CancellationToken.None);

            // Assert
            Assert.True(result.CanProceed);
        }

        [Fact]
        public async Task FailsToAuthorize_WhenNoCertificateMatchesMpc()
        {
            // Arrange
            MessagingContext context = ContextWithPullRequest("not existing pull request mpc");
            var sut = new VerifyPullRequestAuthorizationStep(new StubAuthorizationMap((r, c) => false));

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.ExecuteAsync(context, CancellationToken.None));
        }

        private static MessagingContext ContextWithPullRequest(string expectedMpc)
        {
            AS4Message message = AS4Message.Create(new SendingProcessingMode());
            message.SignalMessages.Add(new PullRequest(expectedMpc));

            return new MessagingContext(message, MessagingContextMode.Send);
        }
    }
}
