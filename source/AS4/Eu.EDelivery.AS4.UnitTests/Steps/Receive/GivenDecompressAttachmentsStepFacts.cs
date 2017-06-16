using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="DecompressAttachmentsStep" />
    /// </summary>
    public class GivenDecompressAttachmentsStepFacts
    {
        /// <summary>
        /// Testing the Step with valid arguments
        /// </summary>
        public class GivenValidArguments : GivenDecompressAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteSucceedsWithValidAttachmentsAsync()
            {
                // Arrange
                MessagingContext context = CompressedAS4Message();

                // Act
                StepResult stepResult = await ExerciseDecompress(context);

                // Assert
                Assert.NotNull(stepResult.MessagingContext.AS4Message.Attachments.First().Content);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithNoCompressedAttachmentAsync()
            {
                // Arrange
                MessagingContext context = CompressedAS4Message();
                context.AS4Message.Attachments.First().ContentType = "not supported MIME type";

                // Act
                StepResult stepResult = await ExerciseDecompress(context);

                // Assert
                Assert.All(
                    stepResult.MessagingContext.AS4Message.Attachments,
                    a => Assert.NotEqual("application/gzip", a.ContentType));
            }
        }

        public class GivenInvalidArguments : GivenDecompressAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteFailsWithMissingMimTypePartPropertyAsync()
            {
                // Act
                MessagingContext context = CompressedAS4Message();
                Attachment attachment = context.AS4Message.Attachments.First();
                attachment.Properties.Remove("MimeType");

                // Assert
                await Assert.ThrowsAsync<AS4Exception>(() => ExerciseDecompress(context));
            }
        }

        private static MessagingContext CompressedAS4Message()
        {
            const string attachmentId = "attachment-id";

            AS4Message as4Message = AS4Message.Create(UserMessageWithCompressedInfo(attachmentId));
            as4Message.AddAttachment(CompressedAttachment(attachmentId));

            return new MessagingContext(as4Message, MessagingContextMode.Unknown);
        }

        private static Attachment CompressedAttachment(string attachmentId)
        {
            Attachment attachment = CreateAttachment(attachmentId);
            CompressAttachment(attachment);
            AssignAttachmentProperties(attachment);

            return attachment;
        }

        private static Attachment CreateAttachment(string id)
        {
            var attachment = new Attachment(id);

            byte[] bytes = Encoding.UTF8.GetBytes("Dummy Attachment Content");
            attachment.Content = new MemoryStream(bytes);

            return attachment;
        }

        private static void CompressAttachment(Attachment attachment)
        {
            var memoryStream = new MemoryStream();
            var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
            attachment.Content.CopyTo(gzipStream);

            memoryStream.Position = 0;
            attachment.Content = memoryStream;
        }

        private static void AssignAttachmentProperties(Attachment attachment)
        {
            attachment.ContentType = "application/gzip";
            attachment.Properties["MimeType"] = "html/text";
        }

        private static UserMessage UserMessageWithCompressedInfo(string attachmentId)
        {
            var properties = new Dictionary<string, string> { ["MimeType"] = "html/text" };
            var partInfo = new PartInfo("cid:" + attachmentId) { Properties = properties };
            var userMessage = new UserMessage("message-id") { PayloadInfo = new List<PartInfo> { partInfo } };

            return userMessage;
        }

        private static async Task<StepResult> ExerciseDecompress(MessagingContext context)
        {
            var sut = new DecompressAttachmentsStep();

            // Act
            return await sut.ExecuteAsync(context, CancellationToken.None);
        }
    }
}