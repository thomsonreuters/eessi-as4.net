using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="ZipAttachmentsStep" />
    /// </summary>
    public class GivenZipAttachmentsStepFacts
    {
        public GivenZipAttachmentsStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenZipAttachmentsStepFacts
        {
            [Fact]
            public async Task ThenStepWillNotZipSingleAttachment()
            {
                // Arrange
                const string contentType = "image/png";
                AS4Message as4Message = AS4Message.Empty;
                as4Message.AddAttachment(new Attachment("attachment-id") { ContentType = contentType });

                var internalMessage = new MessagingContext(as4Message);

                // Act
                await new ZipAttachmentsStep().ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.Equal(contentType, internalMessage.AS4Message.Attachments.First().ContentType);
            }

            [Fact]
            public async Task ThenStepWillZipMultipleAttachments()
            {
                // Arrange
                var internalMessage = new MessagingContext(AS4MessageWithTwoAttachments());

                // Act
                await new ZipAttachmentsStep().ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                ICollection<Attachment> attachments = internalMessage.AS4Message.Attachments;
                Assert.Equal(1, attachments.Count);
                Assert.Equal("application/zip", attachments.First().ContentType);
            }

            private static AS4Message AS4MessageWithTwoAttachments()
            {
                var attachment = new Attachment("attachment-id")
                {
                    Content = new MemoryStream(Encoding.UTF8.GetBytes("Plain Dummy Text")),
                    ContentType = "text/plain"
                };

                AS4Message message = AS4Message.Empty;
                message.AddAttachment(attachment);
                message.AddAttachment(attachment);

                return message;
            }
        }
    }
}