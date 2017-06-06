using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using MimeKit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <seealso cref="AS4Message" />
    /// </summary>
    public class GivenAS4MessageFacts
    {
        private readonly AS4MessageBuilder _builder;

        public GivenAS4MessageFacts()
        {
            _builder = new AS4MessageBuilder();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class AddAttachments
        {
            [Fact]
            public async Task ThenAddAttachmentSucceeds()
            {
                // Arrange
                var submitMessage = new SubmitMessage {Payloads = new[] {new Payload(string.Empty)}};
                var sut = new AS4Message();

                // Act
                await sut.AddAttachments(submitMessage.Payloads, async payload => await Task.FromResult(Stream.Null));

                // Assert
                Assert.NotNull(sut.Attachments);
                Assert.Equal(Stream.Null, sut.Attachments.First().Content);
            }

            [Fact]
            public async Task ThenNoAttachmentsAreAddedWithZeroPayloads()
            {
                // Arrange
                var sut = new AS4Message();

                // Act
                await sut.AddAttachments(new Payload[0],  async payload => await Task.FromResult(Stream.Null));

                // Assert
                Assert.False(sut.HasAttachments);
            }
        }

        public class IsPulling
        {
            [Fact]
            public void IsTrueWhenSignalMessageIsPullRequest()
            {
                // Arrange
                AS4Message as4Message = new AS4MessageBuilder().WithSignalMessage(new PullRequest()).Build();

                // Act
                bool isPulling = as4Message.IsPulling;

                // Assert
                Assert.True(isPulling);
            }
        }

        /// <summary>
        /// Testing if the AS4Message Succeeds
        /// </summary>
        public class GivenAS4MessageSucceeds : GivenAS4MessageFacts
        {
            [Theory]
            [InlineData("mpc")]
            public void ThenSaveToMessageWithoutAttachmentsReturnsSoapMessage(string mpc)
            {
                // Act
                UserMessage userMessage = CreateUserMessage();
                AS4Message message = BuildAS4Message(mpc, userMessage);

                using (var soapStream = new MemoryStream())
                {
                    XmlDocument document = SerializeSoapMessage(message, soapStream);
                    XmlNode envelopeElement = document.DocumentElement;

                    // Assert
                    Assert.NotNull(envelopeElement);
                    Assert.Equal(Constants.Namespaces.Soap12, envelopeElement.NamespaceURI);
                }
            }

            [Theory]
            [InlineData("mpc")]
            public void ThenSaveToPullRequestCorrectlySerialized(string mpc)
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                AS4Message message = BuildAS4Message(mpc, userMessage);

                // Act
                using (var soapStream = new MemoryStream())
                {
                    XmlDocument document = SerializeSoapMessage(message, soapStream);

                    // Assert
                    XmlAttribute mpcAttribute = GetMpcAttribute(document);
                    Assert.Equal(mpc, mpcAttribute?.Value);
                }
            }

            [Theory]
            [InlineData("mpc")]
            public void ThenSaveToMessageWithAttachmentsReturnsMimeMessage(string messageContents)
            {
                // Arrange
                var attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes(messageContents));
                var attachment = new Attachment("attachment-id") {Content = attachmentStream};

                UserMessage userMessage = CreateUserMessage();

                AS4Message message =
                    new AS4MessageBuilder().WithUserMessage(userMessage).WithAttachment(attachment).Build();

                // Act
                AssertMimeMessageIsValid(message);
            }

            private void AssertMimeMessageIsValid(AS4Message message)
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

            private static XmlAttribute GetMpcAttribute(XmlDocument document)
            {
                const string node = "/s:Envelope/s:Header/ebms:Messaging/ebms:SignalMessage/ebms:PullRequest";
                XmlAttributeCollection attributes = document.SelectXmlNode(node).Attributes;

                return attributes?.Cast<XmlAttribute>().FirstOrDefault(x => x.Name == "mpc");
            }

            [Fact]
            public void ThenSaveToUserMessageCorrectlySerialized()
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                AS4Message message = new AS4MessageBuilder().WithUserMessage(userMessage).Build();

                // Act
                using (var soapStream = new MemoryStream())
                {
                    message.ContentType = Constants.ContentTypes.Soap;
                    XmlDocument document = SerializeSoapMessage(message, soapStream);

                    // Assert
                    Assert.NotNull(document.DocumentElement);
                    Assert.Contains("Envelope", document.DocumentElement.Name);
                }
            }
        }

        protected UserMessage CreateUserMessage()
        {
            return new UserMessage("message-id") {CollaborationInfo = {AgreementReference = new AgreementReference()}};
        }

        protected XmlDocument SerializeSoapMessage(AS4Message message, MemoryStream soapStream)
        {
            ISerializer serializer = new SoapEnvelopeSerializer();
            serializer.Serialize(message, soapStream, CancellationToken.None);

            soapStream.Position = 0;
            var document = new XmlDocument();
            document.Load(soapStream);

            return document;
        }

        protected MimeMessage SerializeMimeMessage(AS4Message message, MemoryStream mimeStream)
        {
            ISerializer serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
            serializer.Serialize(message, mimeStream, CancellationToken.None);

            message.ContentType = Constants.ContentTypes.Mime;
            mimeStream.Position = 0;

            return MimeMessage.Load(mimeStream);
        }

        protected AS4Message BuildAS4Message(string mpc, UserMessage userMessage)
        {
            return _builder.WithUserMessage(userMessage).WithPullRequest(mpc).Build();
        }
    }
}