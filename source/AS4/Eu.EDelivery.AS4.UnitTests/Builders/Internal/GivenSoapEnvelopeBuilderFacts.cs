using System;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Internal;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Internal
{
    /// <summary>
    /// Testing the <see cref="SoapEnvelopeBuilder" />
    /// </summary>
    public class GivenSoapEnvelopeBuilderFacts
    {
        private readonly SoapEnvelopeBuilder _builder;

        public GivenSoapEnvelopeBuilderFacts()
        {
            this._builder = new SoapEnvelopeBuilder();
        }

        /// <summary>
        /// Testing if the Builder Succeeds
        /// </summary>
        public class GivenValidArgumentsBuilder : GivenSoapEnvelopeBuilderFacts
        {
            [Fact]
            public void ThenBuilderBreaksDownCorrectly()
            {
                // Arrange
                var messagingHeader = new Messaging();
                XmlNode securityNode = new XmlDocument()
                    .CreateNode(XmlNodeType.Element, "SecurityHeader", Constants.Namespaces.Soap11);

                base._builder
                    .SetMessagingHeader(messagingHeader)
                    .SetSecurityHeader(securityNode).Build();

                // Act
                XmlDocument envelope = this._builder
                    .BreakDown()
                    .SetMessagingHeader(messagingHeader)
                    .Build();
                
                // Assert
                Assert.NotNull(envelope);
                Assert.Equal(1, envelope.FirstChild.FirstChild.ChildNodes.Count);
            }

            [Fact]
            public void ThenBuilderStartsWithEmptyEnvelope()
            {
                // Act
                XmlDocument envelope = this._builder.Build();
                // Assert
                Assert.NotNull(envelope);
                Assert.Equal(0, envelope.FirstChild.ChildNodes.Count);
            }

            [Fact]
            public void ThenResultContainsBody()
            {
                // Arrange
                var messagingHeader = new Messaging();
                string bodySecurityId = $"#body-{Guid.NewGuid()}";
                // Act
                XmlDocument envelope = base._builder
                    .SetMessagingHeader(messagingHeader)
                    .SetMessagingBody(bodySecurityId).Build();
                // Assert
                Assert.NotNull(envelope);
                Assert.Equal("s12:Body", envelope.FirstChild.ChildNodes[1].Name);
                Assert.Equal("http://www.w3.org/2003/05/soap-envelope", envelope.FirstChild.ChildNodes[1].NamespaceURI);
            }

            [Fact]
            public void ThenResultContainsEnvelope()
            {
                // Act
                XmlDocument envelope = base._builder.Build();
                // Assert
                Assert.NotNull(envelope);
                Assert.Equal("s12:Envelope", envelope.FirstChild.Name);
                Assert.Equal("http://www.w3.org/2003/05/soap-envelope", envelope.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenResultContainsHeader()
            {
                // Arrange
                var messagingHeader = new Messaging();
                // Act
                XmlDocument envelope = base._builder
                    .SetMessagingHeader(messagingHeader)
                    .Build();
                // Assert
                Assert.NotNull(envelope);
                Assert.Equal("s12:Header", envelope.FirstChild.FirstChild.Name);
                Assert.Equal("http://www.w3.org/2003/05/soap-envelope", envelope.FirstChild.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenResultContainsSecurityHeader()
            {
                // Arrange
                var messagingHeader = new Messaging();
                XmlNode securityNode = new XmlDocument()
                    .CreateNode(XmlNodeType.Element, "SecurityHeader", Constants.Namespaces.Soap11);
                // Act
                XmlDocument envelope = base._builder
                    .SetMessagingHeader(messagingHeader)
                    .SetSecurityHeader(securityNode)
                    .Build();
                // Assert
                Assert.NotNull(envelope);
                Assert.Equal(securityNode, envelope.FirstChild.FirstChild.ChildNodes[1]);
            }

            [Fact]
            public void ThenResultContainsRoutingInput()
            {
                // Arrange
                var messaging = new Messaging();
                var routingInput = new RoutingInput {UserMessage = CreatePopulatedUserMessage()};
                // Act
                XmlDocument envelope = base._builder
                    .SetMessagingHeader(messaging)
                    .SetRoutingInput(routingInput)
                    .Build();
                // Assert
                Assert.NotNull(envelope);
                XmlNode routingInputNode = envelope.SelectSingleNode("//*[local-name()='RoutingInput']");
                Assert.NotNull(routingInputNode);

            }

            private RoutingInputUserMessage CreatePopulatedUserMessage()
            {
                return new RoutingInputUserMessage
                {
                    MessageInfo = new MessageInfo(),
                    CollaborationInfo = new CollaborationInfo(),
                    PartyInfo = new PartyInfo()
                };
            }
        }
    }
}