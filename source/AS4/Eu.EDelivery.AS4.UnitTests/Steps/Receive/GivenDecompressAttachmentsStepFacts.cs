using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using PartInfo = Eu.EDelivery.AS4.Model.Core.PartInfo;
using Schema = Eu.EDelivery.AS4.Model.Core.Schema;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="DecompressAttachmentsStep" />
    /// </summary>
    public class GivenDecompressAttachmentsStepFacts
    {
        public class GivenValidArguments : GivenDecompressAttachmentsStepFacts
        {
            [CustomProperty]
            public void Bundled_UserMessage_With_Signal_Gets_Decompressed(SignalMessage signal)
            {
                // Arrange
                string attachmentId = $"attachment-{Guid.NewGuid()}";
                UserMessage user = UserMessageWithCompressedInfo(attachmentId);
                Attachment attachment = CompressedAttachment(attachmentId);
                AS4Message as4Message = AS4Message.Create(user);
                as4Message.AddMessageUnit(signal);
                as4Message.AddAttachment(attachment);

                // Act
                StepResult result = ExerciseDecompress(as4Message);

                // Assert
                Assert.All(
                    result.MessagingContext.AS4Message.Attachments,
                    a => Assert.NotEqual("application/gzip", a.ContentType));
            }

            [Property]
            public Property Multiple_UserMessages_Their_Attachments_Gets_Decompressed(NonEmptyArray<Guid> attachmentIds)
            {
                Action act = () =>
                {
                    // Arrange
                    AS4Message as4Message = attachmentIds.Get.Distinct().Aggregate(
                        AS4Message.Empty,
                        (as4, id) =>
                        {
                            as4.AddMessageUnit(UserMessageWithCompressedInfo(id.ToString()));
                            as4.AddAttachment(CompressedAttachment(id.ToString()));
                            return as4;
                        });

                    // Act
                    StepResult result = ExerciseDecompress(as4Message);

                    // Assert
                    Assert.All(
                        result.MessagingContext.AS4Message.Attachments,
                        a => Assert.NotEqual("application/gzip", a.ContentType));
                };

                return act.When(attachmentIds.Get.Distinct().Any());
            }

            private static StepResult ExerciseDecompress(AS4Message as4Message)
            {
                var sut = new DecompressAttachmentsStep();
                return sut.ExecuteAsync(new MessagingContext(as4Message, MessagingContextMode.Receive))
                          .GetAwaiter()
                          .GetResult();
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithValidAttachments()
            {
                // Arrange
                MessagingContext context = CompressedAS4Message();

                // Act
                StepResult stepResult = await ExerciseDecompress(context);

                // Assert
                Assert.NotNull(stepResult.MessagingContext.AS4Message.Attachments.First().Content);
            }

            [Fact]
            public async Task ThenExecuteSucceedsWithNoCompressedAttachment()
            {
                // Arrange
                MessagingContext context = CompressedAS4Message();
                Attachment first = context.AS4Message.Attachments.First();
                first.UpdateContent(first.Content, "not supported MIME type");

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
            public async Task ThenExecuteFailsWithMissingMimTypePartProperty()
            {
                // Arrange
                MessagingContext context = CompressedAS4Message();
                Attachment attachment = context.AS4Message.Attachments.First();
                attachment.Properties.Remove("MimeType");

                // Act
                StepResult result = await ExerciseDecompress(context);
                
                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0303, error.Code);
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
            var memoryStream = new MemoryStream();
            var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
            var input = new MemoryStream(Encoding.UTF8.GetBytes("Dummy Attachment Content"));
            input.CopyTo(gzipStream);

            memoryStream.Position = 0;

            return new Attachment(
                attachmentId,
                memoryStream,
                "application/gzip",
                new Dictionary<string, string> { ["MimeType"] = "html/text" });
        }

        private static UserMessage UserMessageWithCompressedInfo(string attachmentId)
        {
            var partInfo = new PartInfo(
                href: "cid:" + attachmentId,
                properties: new Dictionary<string, string> { ["MimeType"] = "html/text" }, 
                schemas: new Schema[0]);

            var userMessage = new UserMessage($"user-{Guid.NewGuid()}");
            userMessage.AddPartInfo(partInfo);

            return userMessage;
        }

        private static async Task<StepResult> ExerciseDecompress(MessagingContext context)
        {
            var sut = new DecompressAttachmentsStep();

            // Act
            return await sut.ExecuteAsync(context);
        }
    }
}