using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    public class GivenPullVerifySignatureStepFacts
    {
        [Fact]
        public async Task FailsVerifySignature_ResultsInStoppedExecution()
        {
            // Arrange
            Func<Task<StepResult>> act = await SetupExerciseVerificationStepWith(as4_soap_signed_pullrequest);

            // Act
            StepResult result = await act();

            // Assert
            Assert.True(result.CanProceed);
        }

        [Fact]
        public async Task SucceedsVerifySignature_ResultsInSameMessage()
        {
            // Arrange
            Func<Task<StepResult>> act = await SetupExerciseVerificationStepWith(as4_soap_wrong_signed_pullrequest);

            // Act / Assert
            await Assert.ThrowsAnyAsync<PullRequestValidationException>(() => act());
        }

        private static async Task<Func<Task<StepResult>>> SetupExerciseVerificationStepWith(string content)
        {
            // Arrange
            var sut = new PullVerifySignatureStep();
            var context = new MessagingContext(await content.SoapSerialize());

            // Act / Assert
            return async () => await sut.ExecuteAsync(context, CancellationToken.None);
        }
    }
}
