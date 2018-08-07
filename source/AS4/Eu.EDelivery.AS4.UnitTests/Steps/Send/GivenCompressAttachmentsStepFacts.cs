using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <seealso cref="CompressAttachmentsStep" />
    /// </summary>
    public class GivenCompressAttachmentsStepFacts
    {
        [Fact]
        public async Task DoesntCompressAttachments_IfPModeIsNotSetForCompression()
        {
            // Arrange
            Attachment expected = NonCompressedAttachment();
            MessagingContext context = AS4MessageContext(expected, PModeWithoutCompressionSettings());

            // Act
            StepResult result = await ExerciseCompression(context);

            // Assert
            Attachment actual = result.MessagingContext.AS4Message.Attachments.First();

            Assert.Equal(expected.Content, actual.Content);
            Assert.NotEqual("application/gzip", actual.ContentType);
        }

        private static SendingProcessingMode PModeWithoutCompressionSettings()
        {
            return new SendingProcessingMode {MessagePackaging = {UseAS4Compression = false}};
        }

        [Fact]
        public async Task SucceedsToCompressAttachment_IfPModeIsSetForCompression()
        {
            // Arrange
            Attachment nonCompressedAttachment = NonCompressedAttachment();
            long expectedLength = nonCompressedAttachment.Content.Length;
            string expectedType = nonCompressedAttachment.ContentType;

            MessagingContext context = AS4MessageContext(nonCompressedAttachment, PModeWithCompressionSettings());

            // Act
            StepResult result = await ExerciseCompression(context);

            // Assert
            Attachment actual = result.MessagingContext.AS4Message.Attachments.First();

            Assert.NotEqual(expectedLength, actual.Content.Length);
            Assert.Equal(expectedType, actual.Properties["MimeType"]);
            Assert.Equal("application/gzip", actual.ContentType);
        }

        private static Attachment NonCompressedAttachment()
        {
            return new Attachment(
                id: "attachment-id",
                content: new MemoryStream(Encoding.UTF8.GetBytes("compress me!")),
                contentType: "text/plain");
        }

        private static MessagingContext AS4MessageContext(Attachment attachment, SendingProcessingMode pmode)
        {
            AS4Message as4Message = AS4Message.Create(pmode);
            as4Message.AddAttachment(attachment);

            return new MessagingContext(as4Message, MessagingContextMode.Unknown) {SendingPMode = pmode};
        }

        private static SendingProcessingMode PModeWithCompressionSettings()
        {
            return new SendingProcessingMode {MessagePackaging = {UseAS4Compression = true}};
        }

        private static async Task<StepResult> ExerciseCompression(MessagingContext context)
        {
            var sut = new CompressAttachmentsStep();

            // Act
            return await sut.ExecuteAsync(context);
        }
    }
}