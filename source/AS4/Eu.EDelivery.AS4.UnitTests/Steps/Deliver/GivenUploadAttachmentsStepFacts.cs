using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Eu.EDelivery.AS4.UnitTests.Extensions;
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
        public class GivenValidArguments : GivenUploadAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidAttachmentUploaderAsync()
            {
                // Arrange
                const string expectedLocation = "http://path/to/download/attachment";

                var stubUploader = new StubAttachmentUploader(expectedLocation);
                var stubProvider = new StubAttachmentUploaderProvider(stubUploader);
                var step = new UploadAttachmentsStep(stubProvider);

                // Act
                StepResult result = await step.ExecuteAsync(CreateAS4MessageWithAttachment());

                // Assert
                Assert.Collection(
                    result.MessagingContext.AS4Message.Attachments, 
                    a => Assert.Equal(expectedLocation, a.Location));
            }
        }

        public class GivenInvalidArguments : GivenUploadAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsWithFailedAttachmentUploaderAsync()
            {
                // Arrange
                var saboteurProvider = new Mock<IAttachmentUploaderProvider>();
                saboteurProvider.Setup(p => p.Get(It.IsAny<string>()))
                                .Throws(new Exception("Failed to get Uploader"));

                var step = new UploadAttachmentsStep(saboteurProvider.Object);

                // Act / Assert
                await Assert.ThrowsAnyAsync<Exception>(
                    () => step.ExecuteAsync(CreateAS4MessageWithAttachment()));
            }
        }

        protected MessagingContext CreateAS4MessageWithAttachment()
        {
            const string attachmentId = "attachment-id";

            AS4Message as4Message = AS4Message.Create(
                new UserMessage(Guid.NewGuid().ToString())
                {
                    PayloadInfo = {new PartInfo($"cid:{attachmentId}")}
                });

            as4Message.AddAttachment(
                new Attachment(attachmentId) { Content = Stream.Null });

            return new MessagingContext(as4Message, MessagingContextMode.Unknown)
            {
                ReceivingPMode = new ReceivingProcessingMode
                {
                    MessageHandling = {DeliverInformation = {PayloadReferenceMethod = new Method {Type = "FILE"}}}
                }
            };
        }
    }
}