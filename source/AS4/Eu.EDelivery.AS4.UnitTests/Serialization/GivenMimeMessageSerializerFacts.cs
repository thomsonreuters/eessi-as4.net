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
    public class GivenMimeMessageSerializerFacts
    {
        private const string ContentType =
            "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        private readonly MimeMessageSerializer _serializer;

        public GivenMimeMessageSerializerFacts()
        {
            _serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
        }

        /// <summary>
        /// Testing if the Mime Serializer Succeeds
        /// </summary>
        public class GivenMimeMessageSerializerSucceeds : GivenMimeMessageSerializerFacts
        {
            [Fact]
            public async Task ThenAttachmentContentTypeIsNotNullAsync()
            {
                using (Stream messageStream = SerializeAnonymousMessage())
                {
                    // Act
                    AS4Message as4Message = await _serializer.DeserializeAsync(messageStream, ContentType, CancellationToken.None);

                    // Assert
                    Assert.NotNull(as4Message);
                    Assert.Equal(2, as4Message.Attachments.Count);
                }
            }

            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsForContentTypeAsync()
            {
                using (Stream messageStream = SerializeAnonymousMessage())
                {
                    // Act
                    AS4Message as4Message = await _serializer.DeserializeAsync(messageStream, ContentType, CancellationToken.None);

                    // Assert
                    Assert.NotEqual(ContentType, as4Message.ContentType);
                    Assert.Contains(Constants.ContentTypes.Mime, as4Message.ContentType);
                }
            }

            [Fact]
            public void ThenSerializeAS4MessageSucceeds()
            {
                using (Stream messageStream = SerializeAnonymousMessage())
                {
                    // Arrange
                    AS4Message as4Message = CreateAnonymousMessage();

                    // Act
                    _serializer.Serialize(as4Message, messageStream, CancellationToken.None);

                    // Assert
                    Assert.True(messageStream.CanRead);
                    Assert.True(messageStream.Length > 0);
                }
            }

            private static AS4Message CreateAnonymousMessage()
            {
                UserMessage userMessage = CreateUserMessage();
                Attachment attachment = CreateEarthAttachment();

                return new AS4MessageBuilder().WithUserMessage(userMessage).WithAttachment(attachment).Build();
            }

            private static UserMessage CreateUserMessage()
            {
                return new UserMessage("message-id")
                {
                    Receiver = new Party("Receiver", new PartyId()),
                    Sender = new Party("Sender", new PartyId())
                };
            }

            private static Attachment CreateEarthAttachment()
            {
                return new Attachment("attachment-id")
                {
                    Content = new MemoryStream(Encoding.UTF8.GetBytes("attachment-stream")),
                    ContentType = "text/plain"
                };
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
                using (Stream messageStream = SerializeAnonymousMessage())
                {
                    // Act / Assert
                    await Assert.ThrowsAsync<AS4Exception>(
                        () => _serializer.DeserializeAsync(messageStream, Constants.ContentTypes.Mime, CancellationToken.None));
                }
               
            }
        }

        private static Stream SerializeAnonymousMessage()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.as4message));
        }
    }
}