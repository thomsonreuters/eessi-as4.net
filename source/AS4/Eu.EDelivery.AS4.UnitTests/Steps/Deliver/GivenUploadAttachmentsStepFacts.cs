using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model;
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
    /// Testing <see cref="UploadAttachmentsStep"/>
    /// </summary>
    public class GivenUploadAttachmentsStepFacts
    {
        private UploadAttachmentsStep _step;
        private readonly Mock<IAttachmentUploaderProvider> _mockedProvider;

        public GivenUploadAttachmentsStepFacts()
        {
            this._mockedProvider = new Mock<IAttachmentUploaderProvider>();
            var mockedUploader = new Mock<IAttachmentUploader>();
            this._mockedProvider.Setup(p => p.Get(It.IsAny<string>())).Returns(mockedUploader.Object);

            this._step = new UploadAttachmentsStep(this._mockedProvider.Object);
        }

        public class GivenValidArguments : GivenUploadAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidAttachmentUploaderAsync()
            {
                // Arrange
                AS4Message as4Message = base.CreateDefaultAS4Message();
                var internalMessage = new InternalMessage(as4Message);
                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                base._mockedProvider.Verify(u => u.Get(It.IsAny<string>()), Times.Once);
            }
        }

        public class GivenInvalidArguments : GivenUploadAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsWithFailedAttachmentUploaderAsync()
            {
                // Arrange
                SetupFailedAttachmentUploader();
                AS4Message as4Message = base.CreateDefaultAS4Message();
                var internalMessage = new InternalMessage(as4Message);
                // Act / Assert
               await Assert.ThrowsAsync<AS4Exception>(()
                    => base._step.ExecuteAsync(internalMessage, CancellationToken.None));
            }

            private void SetupFailedAttachmentUploader()
            {
                this._mockedProvider
                    .Setup(u => u.Get(It.IsAny<string>()))
                    .Throws(new AS4Exception("Failed to get Uploader"));
                this._step = new UploadAttachmentsStep(this._mockedProvider.Object);
            }
        }

        protected AS4Message CreateDefaultAS4Message()
        {
            return new AS4Message
            {
                ReceivingPMode = new ReceivingProcessingMode()
                {
                    Deliver =
                    {
                        PayloadReferenceMethod = new Method
                        {
                            Type = "FILE"
                        }
                    }
                },
                Attachments = new[] {new Attachment(id: "attachment-id")}
            };
        }
    }
}