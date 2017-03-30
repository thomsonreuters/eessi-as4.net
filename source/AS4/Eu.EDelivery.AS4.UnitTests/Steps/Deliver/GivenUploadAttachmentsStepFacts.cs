using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Uploader;
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
                AS4Message as4Message = CreateDefaultAS4Message();
                var internalMessage = new InternalMessage(as4Message);

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                _mockedProvider.Verify(u => u.Get(It.IsAny<string>()), Times.Once);
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
                AS4Message as4Message = CreateDefaultAS4Message();
                var internalMessage = new InternalMessage(as4Message);

                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(
                    () => _step.ExecuteAsync(internalMessage, CancellationToken.None));
            }
        }

        protected AS4Message CreateDefaultAS4Message()
        {
            return new AS4Message
            {
                ReceivingPMode =
                    new ReceivingProcessingMode {Deliver = {PayloadReferenceMethod = new Method {Type = "FILE"}}},
                Attachments = new[] {new Attachment("attachment-id")}
            };
        }
    }
}