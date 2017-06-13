using System.Threading;
using System.Threading.Tasks;
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
        [Theory]
        [InlineData("equal-mpc", "equal-mpc", true)]
        [InlineData("equal-mpc", "not-equal-mpc", false)]
        public async Task ContinuesExecution_IfMatchedCertificateCanBeFoundForTheMpc(
            string messageMpc,
            string mapMpc,
            bool expected)
        {
            // Arrange
            MessagingContext context = ContextWithPullRequest(messageMpc);

            var stubMap = new StubAuthorizationMap((r, c) => r.Mpc.Equals(mapMpc));
            var sut = new VerifyPullRequestAuthorizationStep(stubMap);

            // Act
            StepResult result = await sut.ExecuteAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(expected, result.CanProceed);
        }

        private static MessagingContext ContextWithPullRequest(string expectedMpc)
        {
            AS4Message message = AS4Message.ForSendingPMode(new SendingProcessingMode());
            message.SignalMessages.Add(new PullRequest(expectedMpc));

            return new MessagingContext(message);
        }
    }
}
