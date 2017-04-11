using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly Mock<IAttachmentUploaderProvider> _mockedProvider;

        private UploadAttachmentsStep _step;

        public GivenUploadAttachmentsStepFacts()
        {
            _mockedProvider = new Mock<IAttachmentUploaderProvider>();
            var mockedUploader = new Mock<IAttachmentUploader>();
            _mockedProvider.Setup(p => p.Get(It.IsAny<string>())).Returns(mockedUploader.Object);

            _step = new UploadAttachmentsStep(_mockedProvider.Object);
        }

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
                StepResult result = await step.ExecuteAsync(new InternalMessage(CreateAS4MessageWithAttachment()), CancellationToken.None);

                // Assert
                Attachment firstAttachment = result.InternalMessage.AS4Message.Attachments.First();
                Assert.Equal(expectedLocation, firstAttachment.Location);
            }
        }

        public class GivenInvalidArguments : GivenUploadAttachmentsStepFacts
        {
            private void SetupFailedAttachmentUploader()
            {
                _mockedProvider.Setup(u => u.Get(It.IsAny<string>())).Throws(new AS4Exception("Failed to get Uploader"));
                _step = new UploadAttachmentsStep(_mockedProvider.Object);
            }

            [Fact]
            public async Task ThenExecuteStepFailsWithFailedAttachmentUploaderAsync()
            {
                // Arrange
                SetupFailedAttachmentUploader();
                AS4Message as4Message = CreateAS4MessageWithAttachment();
                var internalMessage = new InternalMessage(as4Message);

                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(() => _step.ExecuteAsync(internalMessage, CancellationToken.None));
            }
        }

        protected AS4Message CreateAS4MessageWithAttachment()
        {
            return new AS4Message
            {
                ReceivingPMode = new ReceivingProcessingMode {Deliver = {PayloadReferenceMethod = new Method {Type = "FILE"}}},
                Attachments = new[] {new Attachment("attachment-id")}
            };
        }
    }
}