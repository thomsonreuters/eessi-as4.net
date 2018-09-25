using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using FsCheck;
using FsCheck.Xunit;
using MimeKit;
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
                Assert.Equal(2, as4Message.Attachments.Count());
            }

            [Fact]
            public async Task ThenDeserializeAS4MessageSucceedsForContentTypeAsync()
            {
                // Act
                AS4Message as4Message = await ExerciseMimeDeserializeAnonymousUserMessage();

                // Assert
                Assert.Equal(AnonymousContentType, as4Message.ContentType);
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
                return new UserMessage(
                    "message-id",
                    new Party("Sender", new PartyId(Guid.NewGuid().ToString())),
                    new Party("Receiver", new PartyId(Guid.NewGuid().ToString())));
            }

            private static Attachment CreateEarthAttachment()
            {
                return new Attachment(
                    id: "attachment-id",
                    content: new MemoryStream(Encoding.UTF8.GetBytes("attachment-stream")),
                    contentType: "text/plain");
            }

            [Property]
            public void ThenSerializeWithAttachmentsReturnsMimeMessage(NonEmptyString messageContents)
            {
                // Arrange
                var attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes(messageContents.Get));
                var attachment = new Attachment("attachment-id", attachmentStream, "text/plain");

                var userMessage = new UserMessage("message-id");

                AS4Message message = AS4Message.Create(userMessage);
                message.AddAttachment(attachment);

                // Act
                AssertMimeMessageIsValid(message);
            }

            private static void AssertMimeMessageIsValid(AS4Message message)
            {
                using (var mimeStream = new MemoryStream())
                {
                    MimeMessage mimeMessage = SerializeMimeMessage(message, mimeStream);
                    Stream envelopeStream = mimeMessage.BodyParts.OfType<MimePart>().First().ContentObject.Open();
                    string rawXml = new StreamReader(envelopeStream).ReadToEnd();

                    // Assert
                    Assert.NotNull(rawXml);
                    Assert.Contains("Envelope", rawXml);
                }
            }

            private static MimeMessage SerializeMimeMessage(AS4Message message, Stream mimeStream)
            {
                ISerializer serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
                serializer.Serialize(message, mimeStream, CancellationToken.None);

                mimeStream.Position = 0;

                return MimeMessage.Load(mimeStream);
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