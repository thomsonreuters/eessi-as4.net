using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
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
            _builder = new AS4MessageBuilder();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
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
            MimeMessage mimeMessage = MimeMessage.Load(mimeStream);
            return mimeMessage;
        }

        protected AS4Message BuildAS4Message(string mpc, UserMessage userMessage)
        {
            return _builder.WithUserMessage(userMessage).WithPullRequest(mpc).Build();
        }

        public class GetSendConfiguration : GivenAS4MessageFacts, IEnumerable<object[]>
        {
            [Theory]
            [ClassData(typeof(GetSendConfiguration))]
            public void IsExpectedWith(Type expectedType, SignalMessage signalMessage)
            {
                // Arrange
                AS4Message message = new AS4MessageBuilder().WithSignalMessage(signalMessage).Build();

                // Act
                ISendConfiguration sendConfiguration = message.GetSendConfiguration();

                // Assert
                Assert.IsType(expectedType, sendConfiguration);
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {typeof(PullConfiguration), new PullRequest()};
                yield return new object[] {typeof(PushConfiguration), new Receipt()};
            }
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

            [Theory, InlineData("mpc")]
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

            [Theory, InlineData("mpc")]
            public void ThenSaveToMessageWithAttachmentsReturnsMimeMessage(string messageContents)
            {
                // Arrange
                var attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes(messageContents));
                var attachment = new Attachment(id: "attachment-id") {Content = attachmentStream};

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
    }
}