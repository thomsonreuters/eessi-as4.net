using System;
using System.Threading;
using System.Threading.Tasks;
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
            await TestExerciseVerifySignature(as4_soap_wrong_signed_pullrequest, r => Assert.False(r.CanProceed));
        }

        [Fact]
        public async Task SucceedsVerifySignature_ResultsInSameMessage()
        {
            await TestExerciseVerifySignature(as4_soap_signed_pullrequest, r => Assert.True(r.CanProceed));
        }

        private static async Task TestExerciseVerifySignature(string content, Action<StepResult> assertion)
        {
            // Arrange
            var sut = new PullVerifySignatureStep();
            var context = new MessagingContext(await content.SoapSerialize());

            // Act
            StepResult result = await sut.ExecuteAsync(context, CancellationToken.None);

            // Assert
            assertion(result);
        }
    }
}
