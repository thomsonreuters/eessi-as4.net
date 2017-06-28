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
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    /// <summary>
    /// Testing the <see cref="MimeMessageSerializer" />
    /// </summary>
    public class GivenMimeMessageSerializerFacts
    {
        private const string AnonymousContentType =
            "multipart/related; boundary=\"=-M9awlqbs/xWAPxlvpSWrAg==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        protected async Task<AS4Message> ExerciseMimeDeserialize(Stream stream, string contentType)
        {
            // Arrange
            var sut = new MimeMessageSerializer(new SoapEnvelopeSerializer());

            // Act
            return await sut.DeserializeAsync(stream, contentType, CancellationToken.None);
        }

        public class GivenMimeMessageSerializerSucceeds : GivenMimeMessageSerializerFacts
        {
            [Fact]
            public async Task DeserializeMultiHopSignalMessage()
            {
                // Arrange
                const string contentType =
                    "multipart/related; boundary=\"=-M/sMGEhQK8RBNg/21Nf7Ig==\";\ttype=\"application/soap+xml\"";
                string messageString = Encoding.UTF8.GetString(as4_multihop_message).Replace((char)0x1F, ' ');
                byte[] messageContent = Encoding.UTF8.GetBytes(messageString);

                using (var messageStream = new MemoryStream(messageContent))
                {
                    // Act
                    AS4Message actualMessage = await ExerciseMimeDeserialize(messageStream, contentType);

                    // Assert
                    Assert.True(actualMessage.IsSignalMessage);
                }
            }

            [Fact]
            public async Task ThenAttachmentContentTypeIsNotNullAsync()
            {
                // Act
                AS4Message as4Message = await ExerciseMimeDeserializeAnonymousUserMessage();

                // Assert
                Assert.NotNull(as4Message);
                Assert.Equal(2, as4Message.Attachments.Count);
            }

            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsForContentTypeAsync()
            {
                // Act
                AS4Message as4Message = await ExerciseMimeDeserializeAnonymousUserMessage();

                // Assert
                Assert.NotEqual(AnonymousContentType, as4Message.ContentType);
                Assert.Contains(Constants.ContentTypes.Mime, as4Message.ContentType);
            }

            private async Task<AS4Message> ExerciseMimeDeserializeAnonymousUserMessage()
            {
                using (Stream messageStream = SerializeAnonymousMessage())
                {
                    return await ExerciseMimeDeserialize(messageStream, AnonymousContentType);
                }
            }

            [Fact]
            public void ThenSerializeAS4MessageSucceeds()
            {
                using (Stream messageStream = SerializeAnonymousMessage())
                {
                    // Arrange
                    AS4Message as4Message = CreateAnonymousMessage();
                    var sut = new MimeMessageSerializer(new SoapEnvelopeSerializer());

                    // Act
                    sut.Serialize(as4Message, messageStream, CancellationToken.None);

                    // Assert
                    Assert.True(messageStream.CanRead);
                    Assert.True(messageStream.Length > 0);
                }
            }

            private static AS4Message CreateAnonymousMessage()
            {
                AS4Message message = AS4Message.Create(CreateUserMessage());
                message.AddAttachment(CreateEarthAttachment());

                return message;
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

        public class GivenMimeMessageSerializerFails : GivenMimeMessageSerializerFacts
        {
            [Fact]
            public async Task ThenDeserializeFailsWithInvalidContentTypeAsync()
            {
                using (Stream messageStream = SerializeAnonymousMessage())
                {
                    const string notCompleteContentType = Constants.ContentTypes.Mime;

                    // Act / Assert
                    await Assert.ThrowsAnyAsync<Exception>(
                        () => ExerciseMimeDeserialize(messageStream, notCompleteContentType));
                }
            }
        }

        private static Stream SerializeAnonymousMessage()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(as4message));
        }
    }
}