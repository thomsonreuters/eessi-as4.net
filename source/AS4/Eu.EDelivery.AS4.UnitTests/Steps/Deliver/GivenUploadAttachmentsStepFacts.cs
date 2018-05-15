using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Eu.EDelivery.AS4.UnitTests.Strategies.Uploader;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="UploadAttachmentsStep" />
    /// </summary>
    public class GivenUploadAttachmentsStepFacts
    {
        [Fact]
        public async Task Throws_When_Uploading_Attachments_Failed()
        {
            // Arrange
            var sabtoeurProvider = new Mock<IAttachmentUploaderProvider>();
            sabtoeurProvider
                .Setup(p => p.Get(It.IsAny<string>()))
                .Throws(new Exception("Failed to get Uploader"));

            var sut = new UploadAttachmentsStep(sabtoeurProvider.Object);

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.ExecuteAsync(CreateAS4MessageWithAttachment()));
        }

        [Fact]
        public async Task Update_With_Attachment_Location_When_Uploading_Attachments_Succeeds()
        {
            // Arrange
            const string expectedLocation = "http://path/to/download/attachment";

            var stubUploader = new StubAttachmentUploader(expectedLocation);
            var stubProvider = new StubAttachmentUploaderProvider(stubUploader);
            var sut = new UploadAttachmentsStep(stubProvider);

            // Act
            StepResult result = await sut.ExecuteAsync(CreateAS4MessageWithAttachment());

            // Assert
            Assert.Collection(
                result.MessagingContext.AS4Message.Attachments,
                a => Assert.Equal(expectedLocation, a.Location));
        }

        private MessagingContext CreateAS4MessageWithAttachment()
        {
            const string attachmentId = "attachment-id";

            var userMessage = new UserMessage(messageId: Guid.NewGuid().ToString())
            {
                PayloadInfo = { new PartInfo($"cid:{attachmentId}") }
            };
            AS4Message as4Message = AS4Message.Create(userMessage);
            as4Message.AddAttachment(
                new Attachment(attachmentId)
                {
                    Content = Stream.Null
                });

            return new MessagingContext(as4Message, MessagingContextMode.Unknown)
            {
                ReceivingPMode = new ReceivingProcessingMode
                {
                    MessageHandling =
                    {
                        DeliverInformation =
                        {
                            PayloadReferenceMethod = new Method { Type = "FILE" }
                        }
                    }
                }
            };
        }
    }
}