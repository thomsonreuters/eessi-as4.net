using System;
using System.Xml;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Internal
{
    /// <summary>
    /// Testing the <see cref="SoapEnvelopeSerializer.SoapEnvelopeBuilder" />
    /// </summary>
    public class GivenSoapEnvelopeBuilderFacts
    {
        private readonly SoapEnvelopeSerializer.SoapEnvelopeBuilder _builder;

        public GivenSoapEnvelopeBuilderFacts()
        {
            _builder = new SoapEnvelopeSerializer.SoapEnvelopeBuilder(envelopeDocument: null);
        }

        public class GivenValidArgumentsBuilder : GivenSoapEnvelopeBuilderFacts
        {
            [Fact]
            public void Builder_Fails_To_Create_When_Envelope_Element_Is_Missing()
            {
                Assert.Throws<NotSupportedException>(
                    () => new SoapEnvelopeSerializer.SoapEnvelopeBuilder(new XmlDocument()));
            }

            [Fact]
            public void Builder_Fails_To_Create_When_Header_Element_Is_Missing()
            {
                var doc = new XmlDocument();
                XmlElement envelope = doc.CreateElement("s12", "Envelope", Constants.Namespaces.Soap12);
                doc.AppendChild(envelope);

                Assert.Throws<NotSupportedException>(
                    () => new SoapEnvelopeSerializer.SoapEnvelopeBuilder(doc));
            }

            [Fact]
            public void Builder_Fails_To_Create_When_Body_Element_Is_Missing()
            {
                var doc = new XmlDocument();
                XmlElement envelope = doc.CreateElement("s12", "Envelope", Constants.Namespaces.Soap12);
                envelope.AppendChild(doc.CreateElement("s12", "Header", Constants.Namespaces.Soap12));
                doc.AppendChild(envelope);

                Assert.Throws<NotSupportedException>(
                    () => new SoapEnvelopeSerializer.SoapEnvelopeBuilder(doc));
            }

            [Theory]
            [InlineData(Constants.Namespaces.EbmsOneWayReceipt)]
            [InlineData(Constants.Namespaces.EbmsOneWayError)]
            public void ThenResultContainsAction(string action)
            {
                // Act
                XmlDocument envelope = _builder.SetActionHeader(action).Build();

                // Assert
                Assert.NotNull(envelope);
                XmlNode actionNode = envelope.SelectEbmsNode("/s12:Envelope/s12:Header/wsa:Action");
                Assert.Equal(action, actionNode.InnerText);
            }

            [Fact]
            public void ThenBuilderStartsWithEmptyEnvelope()
            {
                // Act
                XmlDocument envelope = _builder.Build();

                // Assert
                Assert.NotNull(envelope);
                XmlNode envelopeNode = envelope.SelectEbmsNode("/s12:Envelope");
                Assert.Empty(envelopeNode.ChildNodes);
            }

            [Fact]
            public void ThenResultContainsBody()
            {
                // Arrange
                string bodySecurityId = $"#body-{Guid.NewGuid()}";

                // Act
                XmlDocument envelope =
                    _builder.SetMessagingBody(bodySecurityId)
                            .Build();

                // Assert
                Assert.NotNull(envelope);
                XmlNode bodyNode = envelope.SelectEbmsNode("/s12:Envelope/s12:Body");
                Assert.Equal("s12:Body", bodyNode.Name);
                Assert.Equal(Constants.Namespaces.Soap12, bodyNode.NamespaceURI);
            }

            [Fact]
            public void ThenResultContainsEnvelope()
            {
                // Act
                XmlDocument envelope = _builder.Build();

                // Assert
                Assert.NotNull(envelope);
                XmlNode envelopeNode = envelope.SelectEbmsNode("/s12:Envelope");
                Assert.Equal("s12:Envelope", envelopeNode.Name);
                Assert.Equal(Constants.Namespaces.Soap12, envelopeNode.NamespaceURI);
            }

            [Fact]
            public void ThenResultDoesntContainsHeader()
            {
                // Act
                XmlDocument envelope = _builder.Build();

                // Assert
                Assert.NotNull(envelope);
                XmlNode headerNode = envelope.UnsafeSelectEbmsNode("/s12:Envelope/s12:Header");
                Assert.Null(headerNode);
            }

            [Fact]
            public void ThenResultContainsRoutingInput()
            {
                // Arrange
                var routingInput = new RoutingInput
                {
                    UserMessage = new RoutingInputUserMessage
                    {
                        MessageInfo = new MessageInfo(),
                        CollaborationInfo = new CollaborationInfo(),
                        PartyInfo = new PartyInfo()
                    }
                };

                // Act
                XmlDocument envelope = 
                    _builder.SetRoutingInput(routingInput)
                            .Build();

                // Assert
                Assert.NotNull(envelope);
                envelope.SelectEbmsNode("/s12:Envelope/s12:Header/mh:RoutingInput");
            }

            [Fact]
            public void ThenResultContainsSecurityHeader()
            {
                // Arrange
                XmlNode securityNode = new XmlDocument().CreateNode(
                    XmlNodeType.Element,
                    "SecurityHeader",
                    Constants.Namespaces.WssSecuritySecExt);

                // Act
                XmlDocument envelope =
                    _builder.SetSecurityHeader(securityNode)
                            .Build();

                // Assert
                Assert.NotNull(envelope);
                envelope.SelectEbmsNode("/s12:Envelope/s12:Header/wsse:SecurityHeader");
            }

            [Fact]
            public void ThenResultContainsTo()
            {
                // Arrange
                var to = new To {Role = Constants.Namespaces.ICloud};

                // Act
                XmlDocument envelope = 
                    _builder.SetToHeader(to)
                            .Build();

                // Assert
                XmlNode toNode = envelope.SelectEbmsNode("/s12:Envelope/s12:Header/wsa:To");
                Assert.Equal(to.Role, toNode.InnerText);
            }
        }
    }
}