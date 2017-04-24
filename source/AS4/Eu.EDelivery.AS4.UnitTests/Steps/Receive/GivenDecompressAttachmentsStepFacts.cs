using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly DecompressAttachmentsStep _step;
        private readonly InternalMessage _internalMessage;
        private readonly string _attachmentId;

        public GivenDecompressAttachmentsStepFacts()
        {
            _attachmentId = "Dummy Attachment Id";
            _step = new DecompressAttachmentsStep();

            var as4Message = new AS4Message();
            AddAttachment(as4Message);
            AddUserMessage(as4Message);
            _internalMessage = new InternalMessage(as4Message);
        }

        private void AddAttachment(AS4Message as4Message)
        {
            Attachment attachment = CreateAttachment();
            CompressAttachment(attachment);
            AssignAttachmentProperties(attachment);
            as4Message.AddAttachment(attachment);
        }

        private Attachment CreateAttachment()
        {
            var attachment = new Attachment(_attachmentId);
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

        private void AddUserMessage(AS4Message as4Message)
        {
            var properties = new Dictionary<string, string> {["MimeType"] = "html/text"};
            var partInfo = new PartInfo("cid:" + _attachmentId) {Properties = properties};
            var userMessage = new UserMessage("message-id") {PayloadInfo = new List<PartInfo> {partInfo}};

            as4Message.UserMessages.Add(userMessage);
        }

        /// <summary>
        /// Testing the Step with valid arguments
        /// </summary>
        public class GivenValidArguments : GivenDecompressAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteSucceedsWithValidAttachmentsAsync()
            {
                // Arrange
                var as4Message = new AS4Message();
                AddAttachment(as4Message);
                AddUserMessage(as4Message);
                var internalMessage = new InternalMessage(as4Message);
                var step = new DecompressAttachmentsStep();

                // Act
                StepResult stepResult = await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.NotNull(stepResult.InternalMessage.AS4Message.Attachments.First().Content);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithNoCompressedAttachmentAsync()
            {
                // Arrange
                var attachments = (IList<Attachment>)_internalMessage.AS4Message.Attachments;

                foreach (Attachment attachment1 in attachments)
                {
                    attachment1.ContentType = "not supported MIME type";
                }

                // Act
                StepResult stepResult = await _step.ExecuteAsync(_internalMessage, CancellationToken.None);

                // Assert
                Attachment attachment = GetAssertAttachment(stepResult);
                Assert.NotEqual("application/gzip", attachment.ContentType);
            }

            private static Attachment GetAssertAttachment(StepResult stepResult)
            {
                var attachments = (IList<Attachment>)stepResult.InternalMessage.AS4Message.Attachments;
                Assert.NotNull(attachments);
                Attachment attachment = attachments[0];
                Assert.NotNull(attachment.Content);

                return attachment;
            }
        }

        public class GivenInvalidArguments : GivenDecompressAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteFailsWithMissingMimTypePartPropertyAsync()
            {
                // Act
                Attachment attachment = _internalMessage.AS4Message.Attachments.First();
                attachment.Properties.Remove("MimeType");

                // Assert
                await Assert.ThrowsAsync<AS4Exception>(
                    () => _step.ExecuteAsync(_internalMessage, CancellationToken.None));
            }
        }
    }
}