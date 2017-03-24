using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.Utilities;
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
            this._builder = new AS4MessageBuilder();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        protected UserMessage CreateUserMessage()
        {
            return new UserMessage("message-id") { CollaborationInfo = { AgreementReference = new AgreementReference() } };
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
            MimeMessage mimeMessage = MimeMessage.Load(mimeStream);
            return mimeMessage;
        }

        protected AS4Message BuildAS4Message(string mpc, UserMessage userMessage)
        {
            return this._builder
                .WithUserMessage(userMessage)
                .WithPullRequest(mpc)
                .Build();
        }

        /// <summary>
        /// Testing if the AS4Message Succeeds
        /// </summary>
        public class GivenAS4MessageSucceeds : GivenAS4MessageFacts
        {
            [Theory, InlineData("mpc")]
            public void ThenSaveToMessageWithoutAttachmentsReturnsSoapMessage(string mpc)
            {
                // Act
                UserMessage userMessage = base.CreateUserMessage();
                AS4Message message = base.BuildAS4Message(mpc, userMessage);

                using (var soapStream = new MemoryStream())
                {
                    XmlDocument document = base.SerializeSoapMessage(message, soapStream);
                    XmlNode envelopeElement = document.DocumentElement;

                    // Assert
                    Assert.NotNull(envelopeElement);
                    Assert.Equal(Constants.Namespaces.Soap12, envelopeElement.NamespaceURI);
                }
            }

            [Theory, InlineData("mpc")]
            public void ThenSaveToPullRequestCorrectlySerialized(string mpc)
            {
                // Arrange
                UserMessage userMessage = base.CreateUserMessage();
                AS4Message message = base.BuildAS4Message(mpc, userMessage);

                // Act
                using (var soapStream = new MemoryStream())
                {
                    XmlDocument document = base.SerializeSoapMessage(message, soapStream);

                    // Assert
                    XmlAttribute mpcAttribute = GetMpcAttribute(document);
                    Assert.Equal(mpc, mpcAttribute?.Value);
                }
            }

            [Theory, InlineData("mpc")]
            public void ThenSaveToMessageWithAttachmentsReturnsMimeMessage(string messageContents)
            {
                // Arrange
                var attachmentStream = new MemoryStream(
                    Encoding.UTF8.GetBytes(messageContents));
                var attachment = new Attachment(id: "attachment-id") {Content = attachmentStream};

                UserMessage userMessage = base.CreateUserMessage();

                AS4Message message = new AS4MessageBuilder()
                    .WithUserMessage(userMessage)
                    .WithAttachment(attachment)
                    .Build();

                // Act
                AssertMimeMessageIsValid(message);
            }

            private void AssertMimeMessageIsValid(AS4Message message)
            {
                using (var mimeStream = new MemoryStream())
                {
                    MimeMessage mimeMessage = base.SerializeMimeMessage(message, mimeStream);
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
                UserMessage userMessage = base.CreateUserMessage();
                AS4Message message = new AS4MessageBuilder()
                    .WithUserMessage(userMessage)
                    .Build();

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

        public class IsUserMessage : GivenAS4MessageFacts
        {
            [Fact]
            public void IsTrueWhenMessageContainsUserMessage()
            {
                // Arrange
                AS4Message as4Message = base.BuildAS4Message("mpc", CreateUserMessage());
                
                // Act
                bool isUserMessage = as4Message.IsUserMessage;

                // Assert
                Assert.True(isUserMessage);
            }
        }
    }
}