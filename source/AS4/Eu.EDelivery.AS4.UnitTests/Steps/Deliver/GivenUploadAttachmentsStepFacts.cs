using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
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
                StepResult result = await step.ExecuteAsync(CreateAS4MessageWithAttachment(), CancellationToken.None);

                // Assert
                Attachment firstAttachment = result.MessagingContext.AS4Message.Attachments.First();
                Assert.Equal(expectedLocation, firstAttachment.Location);
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
                                .Throws(new AS4Exception("Failed to get Uploader"));

                var step = new UploadAttachmentsStep(saboteurProvider.Object);

                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(
                    () => step.ExecuteAsync(CreateAS4MessageWithAttachment(), CancellationToken.None));
            }
        }

        protected MessagingContext CreateAS4MessageWithAttachment()
        {
            AS4Message as4Message = AS4Message.Empty;
            as4Message.AddAttachment(new Attachment("attachment-id") {Content = Stream.Null});

            return new MessagingContext(as4Message)
            {
                ReceivingPMode =
                    new ReceivingProcessingMode {Deliver = {PayloadReferenceMethod = new Method {Type = "FILE"}}}
            };
        }
    }
}