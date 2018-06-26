using Eu.EDelivery.AS4.Functional;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    /// <summary>
    /// Testing <see cref="CollaborationInfoMap"/>
    /// </summary>
    public class GivenCollaborationInfoMapFacts
    {
        [Fact]
        public void SucceedsFromXmlToCore_IfAgreementRefIsFilled()
        {
            // Arrange
            const string expectedRef = "http://agreements.holodeckb2b.org/examples/agreement1";
            var expectedInfo = new CollaborationInfo
            {
                AgreementRef = new AgreementRef {Value = expectedRef}
            };

            // Act
            var actualInfo = AS4Mapper.Map<AS4.Model.Core.CollaborationInfo>(expectedInfo);

            // Assert
            Assert.Equal(expectedRef, actualInfo.AgreementReference.Value);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfActionIsFilled()
        {
            // Arrange
            var expectedInfo = new CollaborationInfo {Action = "StoreMessage"};

            // Act
            var actualInfo = AS4Mapper.Map<AS4.Model.Core.CollaborationInfo>(expectedInfo);

            // Assert
            Assert.Equal(expectedInfo.Action, actualInfo.Action);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfServiceIsFilled()
        {
            // Arrange
            var expectedInfo = new CollaborationInfo {Service = new Service {type = "org:holodeckb2b:services", Value = "Examples"}};

            // Act
            var actualInfo = AS4Mapper.Map<AS4.Model.Core.CollaborationInfo>(expectedInfo);

            // Assert
            Service expectedService = expectedInfo.Service;
            AS4.Model.Core.Service actualService = actualInfo.Service;

            Assert.Equal(Maybe.Just(expectedService.type), actualService.Type);
            Assert.Equal(expectedService.Value, actualService.Value);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfConversationId()
        {
            // Arrange
            var expectedInfo = new CollaborationInfo {ConversationId = "org:holodeckb2b:example:conversation"};

            // Act
            var actualInfo = AS4Mapper.Map<AS4.Model.Core.CollaborationInfo>(expectedInfo);

            // Assert
            Assert.Equal(expectedInfo.ConversationId, actualInfo.ConversationId);
        }
    }
}
