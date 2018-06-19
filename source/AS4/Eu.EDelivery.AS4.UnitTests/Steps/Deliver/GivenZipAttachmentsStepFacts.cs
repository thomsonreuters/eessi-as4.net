using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="ZipAttachmentsStep" />
    /// </summary>
    public class GivenZipAttachmentsStepFacts
    {

        public class GivenValidArguments : GivenZipAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenStepWillNotZipSingleAttachment()
            {
                // Arrange
                const string contentType = "image/png";

                AS4Message as4Message = AS4Message.Empty;
                as4Message.AddAttachment(new Attachment("attachment-id") { ContentType = contentType });

                // Act
                await new ZipAttachmentsStep().ExecuteAsync(
                    new MessagingContext(as4Message, MessagingContextMode.Unknown));

                // Assert
                Assert.Collection(
                    as4Message.Attachments,
                    a => Assert.Equal(contentType, a.ContentType));
            }

            [Fact]
            public async Task ThenStepWillZipMultipleAttachments()
            {
                // Arrange
                AS4Message as4Message = AS4MessageWithTwoAttachments();

                // Act
                await new ZipAttachmentsStep().ExecuteAsync(
                    new MessagingContext(as4Message, MessagingContextMode.Unknown));

                // Assert
                Assert.Collection(
                    as4Message.Attachments,
                    a => Assert.Equal("application/zip", a.ContentType));
            }

            private static AS4Message AS4MessageWithTwoAttachments()
            {
                Attachment CreateAttachment()
                {
                    return new Attachment("attachment" + Guid.NewGuid())
                    {
                        Content = new MemoryStream(Encoding.UTF8.GetBytes("Plain Dummy Text")),
                        ContentType = "text/plain"
                    };
                }

                AS4Message message = AS4Message.Empty;
                message.AddAttachment(CreateAttachment());
                message.AddAttachment(CreateAttachment());

                return message;
            }
        }
    }
}