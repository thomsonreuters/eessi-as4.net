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
            AS4Message message = AS4Message.Create(new UserMessage
            {
                PayloadInfo = new[] {new PartInfo("cid:earth")}
            });
            message.AddAttachment(new Attachment("earth") {Content = Stream.Null});
            message = await SerializeDeserialize(message);
            message.PrimaryUserMessage.PayloadInfo.First().Href = null;

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
            AS4Message message = AS4Message.Create(new UserMessage
            {
                PayloadInfo = new[] {new PartInfo("cid:some other href")}
            });
            message.AddAttachment(new Attachment("earth") {Content = Stream.Null});
            message = await SerializeDeserialize(message);

            // Act
            StepResult result = await ExerciseValidation(message);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(ErrorCode.Ebms0009, result.MessagingContext.ErrorResult.Code);
        }

        [Fact]
        public async Task ValidationFailure_IfSoapBodyAttachmentFound()
        {
            const string contentType = "application/soap+xml";
            AS4Message message = await BuildMessageFor(as4_soapattachment, contentType);

            StepResult result = await ExerciseValidation(message);

            Assert.False(result.Succeeded);
            Assert.Equal(ErrorAlias.FeatureNotSupported, result.MessagingContext.ErrorResult.Alias);
        }

        [Fact]
        public async Task ValidationSucceeds_IfSoapBodyHasNoAttachment()
        {
            const string contentType = "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";
            AS4Message message = await BuildMessageFor(System.Text.Encoding.UTF8.GetBytes(as4message), contentType);

            StepResult result = await ExerciseValidation(message);

            Assert.True(result.Succeeded);            
        }

        [Fact]
        public async Task ValidationFailure_IfUserMessageContainsDuplicatePayloadIds()
        {
            AS4Message message = AS4Message.Create(new UserMessage
            {
                PayloadInfo = new[]
                {
                    new PartInfo("cid:earth"),
                    new PartInfo("cid:earth")
                }
            });

            message.AddAttachment(new Attachment("earth") {Content = Stream.Null});
            message = await SerializeDeserialize(message);

            StepResult result = await ExerciseValidation(message);

            Assert.False(result.Succeeded);
            Assert.Equal(ErrorAlias.InvalidHeader, result.MessagingContext.ErrorResult.Alias);
        }

        private static async Task<AS4Message> SerializeDeserialize(AS4Message message)
        {
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            var memory = new MemoryStream();
            serializer.Serialize(message, memory, CancellationToken.None);
            memory.Position = 0;

            return await serializer.DeserializeAsync(memory, message.ContentType, CancellationToken.None);
        }

        private static async Task<AS4Message> BuildMessageFor(byte[] as4MessageExternalPayloads, string contentType)
        {
            using (var stream = new MemoryStream(as4MessageExternalPayloads))
            {
                var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
                return await serializer.DeserializeAsync(stream, contentType, CancellationToken.None);
            }
        }

        private static async Task<StepResult> ExerciseValidation(AS4Message message)
        {
            var sut = new ValidateAS4MessageStep();

            return await sut.ExecuteAsync(new MessagingContext(message, MessagingContextMode.Receive));
        }
    }
}

