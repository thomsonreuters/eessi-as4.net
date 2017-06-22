using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenValidateAS4MessageStepFacts
    {
        [Fact]
        public async Task ValidationFailure_IfExternalPayloadReference()
        {
            // Arrange
            const string contentType = "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";
            AS4Message message = await BuildMessageFor(as4message_external_payloads, contentType);

            // Act
            StepResult result = await ExerciseValidation(message);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(ErrorCode.Ebms0011, result.MessagingContext.ErrorResult.Code);
        }

        [Fact]
        public async Task ValidationFailure_IfNoAttachmentCanBeFoundForEachPartInfo()
        {
            // Arrange
            const string contentType = "multipart/related; boundary=\"=-PHQq1fuE9QxpIWax7CKj5w==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";
            AS4Message message = await BuildMessageFor(as4_single_payload, contentType);
            message.UserMessages.First().PayloadInfo.First().Href = "cid:some other href";

            // Act
            StepResult result = await ExerciseValidation(message);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(ErrorCode.Ebms0009, result.MessagingContext.ErrorResult.Code);
        }

        private static async Task<AS4Message> BuildMessageFor(byte[] as4MessageExternalPayloads, string contentType)
        {
            using (var stream = new MemoryStream(as4MessageExternalPayloads))
            {
                var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
                return await serializer.DeserializeAsync(stream, contentType, CancellationToken.None);
            }
        }

        private async Task<StepResult> ExerciseValidation(AS4Message message)
        {
            var sut = new ValidateAS4MessageStep();

           return await sut.ExecuteAsync(new MessagingContext(message, MessagingContextMode.Receive), CancellationToken.None);
        }
    }
}

