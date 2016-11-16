using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// Testing the <see cref="MimeMessageSerializer" />
    /// </summary>
    public class GivenMimeMessageSerializerFacts : IDisposable
    {
        private const string ContentType = "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";
        private readonly AS4Message _message;
        private readonly MimeMessageSerializer _serializer;
        protected MemoryStream MemoryStream;

        public GivenMimeMessageSerializerFacts()
        {
            this.MemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.as4message));
            this._serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
            UserMessage userMessage = CreateUserMessage();
            Attachment attachment = CreateEarthAttachment();

            this._message = new AS4MessageBuilder()
                .WithUserMessage(userMessage)
                .WithAttachment(attachment)
                .Build();
        }

        private UserMessage CreateUserMessage()
        {
            return new UserMessage(messageId: "message-id")
            {
                Receiver = new Party("Receiver", new PartyId()),
                Sender = new Party("Sender", new PartyId())
            };
        }

        private Attachment CreateEarthAttachment()
        {
            return new Attachment(id: "attachment-id")
            {
                Content = new MemoryStream(Encoding.UTF8.GetBytes("attachment-stream")),
                ContentType = "text/plain"
            };
        }

        /// <summary>
        /// Testing if the Mime Serializer Succeeds
        /// </summary>
        public class GivenMimeMessageSerializerSucceeds : GivenMimeMessageSerializerFacts
        {
            [Fact]
            public void ThenSerializeAS4MessageSucceeds()
            {
                // Act
                base._serializer.Serialize(this._message, base.MemoryStream, CancellationToken.None);
                // Assert
                Assert.True(base.MemoryStream.CanRead);
                Assert.True(base.MemoryStream.Length > 0);
            }

            [Fact]
            public async Task ThenAttachmentContentTypeIsNotNullAsync()
            {
                // Act
                AS4Message as4Message = await base._serializer.DeserializeAsync(base.MemoryStream, ContentType, CancellationToken.None);
                // Assert
                Assert.NotNull(as4Message);
                Assert.Equal(2, as4Message.Attachments.Count);
            }

            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsForContentTypeAsync()
            {
                // Act
                AS4Message as4Message = await base._serializer.DeserializeAsync(base.MemoryStream, ContentType, CancellationToken.None);
                // Assert
                Assert.NotEqual(ContentType, as4Message.ContentType);
                Assert.Contains(Constants.ContentTypes.Mime, as4Message.ContentType);
            }
        }

        /// <summary>
        /// Testing if the Mime Serializer fails
        /// with invalid arguments
        /// </summary>
        public class GivenMimeMessageSerializerFails : GivenMimeMessageSerializerFacts
        {
            [Fact]
            public async Task ThenDeserializeFailsWithInvalidContentTypeAsync()
            {
                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(() => base._serializer
                    .DeserializeAsync(base.MemoryStream, Constants.ContentTypes.Mime, CancellationToken.None));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.MemoryStream.Dispose();
        }
    }
}