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
                XmlDocument envelope =
                    base._builder.SetMessagingHeader(messagingHeader).SetMessagingBody(bodySecurityId).Build();

                // Assert
                Assert.NotNull(envelope);
                Assert.Equal("s12:Body", envelope.FirstChild.ChildNodes[1].Name);
                Assert.Equal(Constants.Namespaces.Soap12, envelope.FirstChild.ChildNodes[1].NamespaceURI);
            }

            [Fact]
            public void ThenResultContainsEnvelope()
            {
                // Act
                XmlDocument envelope = base._builder.Build();

                // Assert
                Assert.NotNull(envelope);
                Assert.Equal("s12:Envelope", envelope.FirstChild.Name);
                Assert.Equal(Constants.Namespaces.Soap12, envelope.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenResultContainsHeader()
            {
                // Arrange
                var messagingHeader = new Messaging();

                // Act
                XmlDocument envelope = base._builder.SetMessagingHeader(messagingHeader).Build();

                // Assert
                Assert.NotNull(envelope);
                Assert.Equal("s12:Header", envelope.FirstChild.FirstChild.Name);
                Assert.Equal(Constants.Namespaces.Soap12, envelope.FirstChild.FirstChild.NamespaceURI);
            }

            [Fact]
            public void ThenResultContainsSecurityHeader()
            {
                // Arrange
                var messagingHeader = new Messaging();
                XmlNode securityNode = new XmlDocument().CreateNode(
                    XmlNodeType.Element,
                    "SecurityHeader",
                    Constants.Namespaces.Soap11);

                // Act
                XmlDocument envelope =
                    base._builder.SetMessagingHeader(messagingHeader).SetSecurityHeader(securityNode).Build();

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
                XmlDocument envelope = base._builder.SetMessagingHeader(messaging).SetRoutingInput(routingInput).Build();

                // Assert
                Assert.NotNull(envelope);
                Assert.NotNull(SelectSingleNode(envelope, "RoutingInput"));
            }

            [Theory]
            [InlineData("http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/oneWay.receipt")]
            [InlineData("http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/oneWay.error")]
            public void ThenResultContainsAction(string action)
            {
                // Arrange
                var messaging = new Messaging();

                // Act
                XmlDocument envelope = base._builder.SetMessagingHeader(messaging).SetActionHeader(action).Build();

                // Assert
                XmlNode actionNode = SelectSingleNode(envelope, "Action");
                Assert.NotNull(actionNode);
                Assert.Equal(action, actionNode.InnerText);
            }

            [Fact]
            public void ThenResultContainsTo()
            {
                // Arrange
                var messaging = new Messaging();
                var to = new To {Role = Constants.Namespaces.ICloud};

                // Act
                XmlDocument envelope = base._builder.SetMessagingHeader(messaging).SetToHeader(to).Build();

                // Assert
                XmlNode toNode = SelectSingleNode(envelope, "To");
                Assert.NotNull(toNode);
                Assert.Equal(to.Role, toNode.InnerText);
            }

            private static XmlNode SelectSingleNode(XmlNode envelope, string localName)
            {
                return envelope.SelectSingleNode($"//*[local-name()='{localName}']");
            }

            private static RoutingInputUserMessage CreatePopulatedUserMessage()
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