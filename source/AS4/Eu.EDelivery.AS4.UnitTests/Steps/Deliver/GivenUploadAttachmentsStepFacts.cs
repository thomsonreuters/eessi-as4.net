using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Eu.EDelivery.AS4.UnitTests.Strategies.Uploader;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="UploadAttachmentsStep" />
    /// </summary>
    public class GivenUploadAttachmentsStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task Throws_When_Uploading_Attachments_Failed()
        {
            // Arrange
            var sabtoeurProvider = new Mock<IAttachmentUploaderProvider>();
            sabtoeurProvider
                .Setup(p => p.Get(It.IsAny<string>()))
                .Throws(new Exception("Failed to get Uploader"));

            IStep sut = new UploadAttachmentsStep(sabtoeurProvider.Object, GetDataStoreContext);

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.ExecuteAsync(CreateAS4MessageWithAttachment()));
        }

        [Theory]
        [ClassData(typeof(UploadRetryData))]
        public async Task Retries_Uploading_When_Uploader_Returns_RetryableFail_Result(UploadRetry input)
        {
            // Arrange
            string id = "deliver-" + Guid.NewGuid();
            InsertInMessage(id, input.CurrentRetryCount, input.MaxRetryCount);

            var a = new FilledAttachment();
            var userMessage = new FilledUserMessage(id, a.Id);
            AS4Message as4Msg = AS4Message.Create(userMessage);
            as4Msg.AddAttachment(a);

            IAttachmentUploader stub = CreateStubAttachmentUploader(userMessage, input.UploadResult);

            // Act
            await CreateUploadStep(stub)
                .ExecuteAsync(new MessagingContext(as4Msg, MessagingContextMode.Deliver)
                {
                    ReceivingPMode = CreateReceivingPModeWithPayloadMethod()
                });

            // Assert
            GetDataStoreContext.AssertInMessage(id, actual =>
            {
                Assert.NotNull(actual);

                Assert.Equal(input.ExpectedCurrentRetryCount, actual.CurrentRetryCount);
                Assert.Equal(input.ExpectedStatus, InStatusUtils.Parse(actual.Status));
                Assert.Equal(input.ExpectedOperation, OperationUtils.Parse(actual.Operation));
            });
        }

        [Theory]
        [ClassData(typeof(UploadRetryData))]
        public async Task All_Attachments_Should_Succeed_Or_Fail(UploadRetry input)
        {
            // Arrange
            string id = "deliver-" + Guid.NewGuid();
            InsertInMessage(id, input.CurrentRetryCount, input.MaxRetryCount);

            var a1 = new FilledAttachment("attachment-1");
            var a2 = new FilledAttachment("attachment-2");
            var userMessage = new FilledUserMessage(id, a1.Id, a2.Id);
            var as4Msg = AS4Message.Create(userMessage);
            as4Msg.AddAttachment(a1);
            as4Msg.AddAttachment(a2);

            var stub = new Mock<IAttachmentUploader>();
            stub.Setup(s => s.UploadAsync(a1, userMessage))
                .ReturnsAsync(input.UploadResult);
            stub.Setup(s => s.UploadAsync(a2, userMessage))
                .ReturnsAsync(
                    input.UploadResult.Status == SendResult.Success
                        ? UploadResult.FatalFail
                        : UploadResult.RetryableFail);

            // Act
            await CreateUploadStep(stub.Object)
                .ExecuteAsync(new MessagingContext(as4Msg , MessagingContextMode.Deliver)
                {
                    ReceivingPMode = CreateReceivingPModeWithPayloadMethod()
                });

            // Assert
            GetDataStoreContext.AssertInMessage(id, actual =>
            {
                Assert.NotNull(actual);
                Operation op = OperationUtils.Parse(actual.Operation);
                Assert.NotEqual(Operation.Delivered, op);
                InStatus st = InStatusUtils.Parse(actual.Status);
                Assert.NotEqual(InStatus.Delivered, st);

                bool operationToBeDelivered = Operation.ToBeDelivered == op;
                bool uploadResultCanBeRetried =
                    input.UploadResult.Status == SendResult.RetryableFail 
                    && input.CurrentRetryCount < input.MaxRetryCount;

                Assert.True(
                    operationToBeDelivered == uploadResultCanBeRetried,
                    "InMessage should update Operation=ToBeDelivered");

                bool messageSetToException = Operation.DeadLettered == op && InStatus.Exception == st;
                bool exhaustRetries =
                    input.CurrentRetryCount == input.MaxRetryCount
                    || input.UploadResult.Status != SendResult.RetryableFail;
                Assert.True(
                    messageSetToException == exhaustRetries,
                    $"{messageSetToException} != {exhaustRetries} InMessage should update Operation=DeadLettered, Status=Exception");
            });
        }

        private void InsertInMessage(string id, int current, int max)
        {
            var inMsg = new InMessage(id)
            {
                CurrentRetryCount = current,
                MaxRetryCount = max
            };
            inMsg.SetStatus(InStatus.Received);
            inMsg.SetOperation(Operation.Delivering);
            GetDataStoreContext.InsertInMessage(inMsg);
        }

        private static IAttachmentUploader CreateStubAttachmentUploader(UserMessage m, UploadResult r)
        {
            var stub = new Mock<IAttachmentUploader>();
            stub.Setup(s => s.UploadAsync(It.IsAny<Attachment>(), m))
                .ReturnsAsync(r);

            return stub.Object;
        }

        [Fact]
        public async Task Update_With_Attachment_Location_When_Uploading_Attachments_Succeeds()
        {
            // Arrange
            const string expectedLocation = "http://path/to/download/attachment";
            var stubUploader = new StubAttachmentUploader(expectedLocation);

            // Act
            StepResult result = 
                await CreateUploadStep(stubUploader)
                    .ExecuteAsync(CreateAS4MessageWithAttachment());

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
            ReceivingProcessingMode pMode = CreateReceivingPModeWithPayloadMethod();
            return new MessagingContext(as4Message, MessagingContextMode.Unknown)
            {
                ReceivingPMode = pMode
            };
        }

        private static ReceivingProcessingMode CreateReceivingPModeWithPayloadMethod()
        {
            return new ReceivingProcessingMode
            {
                MessageHandling =
                {
                    DeliverInformation =
                    {
                        PayloadReferenceMethod = new Method { Type = "FILE" }
                    }
                }
            };
        }

        /// <summary>
        /// Creates the upload step.
        /// </summary>
        /// <param name="uploader">The uploader.</param>
        /// <returns></returns>
        private IStep CreateUploadStep(IAttachmentUploader uploader)
        {
            return new UploadAttachmentsStep(new StubAttachmentUploaderProvider(uploader), GetDataStoreContext);
        }
    }
}