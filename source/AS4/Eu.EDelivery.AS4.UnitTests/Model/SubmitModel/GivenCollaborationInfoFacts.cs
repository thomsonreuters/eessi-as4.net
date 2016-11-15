using Eu.EDelivery.AS4.Model.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model.SubmitModel
{
    /// <summary>
    /// Testing <see cref="CollaborationInfo"/>
    /// </summary>
    public class GivenCollaborationInfoFacts
    {
        protected CollaborationInfo CreateCollaborationInfo()
        {
            return new CollaborationInfo
            {
                Action = "shared-action",
                ConversationId = "shared-conversation-id",
                Service = new Service(),
                AgreementRef = new Agreement()
            };
        }

        public class GivenValidArguments : GivenCollaborationInfoFacts
        {
            [Fact]
            public void ThenCollaborationInfoHasDefaults()
            {
                // Act
                var collaborationInfo = new CollaborationInfo();
                // Assert
                Assert.NotNull(collaborationInfo.AgreementRef);
                Assert.Null(collaborationInfo.Action);
            }

            [Fact]
            public void ThenTwoCollaborationInfosAreEqual()
            {
                // Arrange
                CollaborationInfo collaborationInfoA = base.CreateCollaborationInfo();
                CollaborationInfo collaborationInfoB = collaborationInfoA;
                // Act
                bool isEqual = collaborationInfoA.Equals(collaborationInfoB);
                // Assert
                Assert.True(isEqual);
            }

            [Fact]
            public void ThenTwoCollaborationInfosAreEqualForProperties()
            {
                // Arrange
                CollaborationInfo collaborationInfoA = base.CreateCollaborationInfo();
                CollaborationInfo collaborationInfoB = base.CreateCollaborationInfo();
                // Act
                bool isEqual = collaborationInfoA.Equals(collaborationInfoB);
                // Assert
                Assert.True(isEqual);
            }

            [Fact]
            public void ThenTwoCollaborationInfosAreEqualForObject()
            {
                // Arrange
                CollaborationInfo collaborationInfoA = base.CreateCollaborationInfo();
                CollaborationInfo collaborationInfoB = base.CreateCollaborationInfo();
                // Act
                bool isEqual = collaborationInfoA.Equals((object) collaborationInfoB);
                // Assert
                Assert.True(isEqual);
            }

            [Fact]
            public void ThenTwoCollaborationInfosAreNotEqualForAction()
            {
                // Arrange
                CollaborationInfo collaborationInfoA = base.CreateCollaborationInfo();
                CollaborationInfo collaborationInfoB = base.CreateCollaborationInfo();
                collaborationInfoB.Action = "not-equal";
                // Act
                bool isEqual = collaborationInfoA.Equals(collaborationInfoB);
                // Assert
                Assert.False(isEqual);
            }

            [Fact]
            public void ThenTwoCollaborationInfosAreNotEqualForConversationId()
            {
                // Arrange
                CollaborationInfo collaborationInfoA = base.CreateCollaborationInfo();
                CollaborationInfo collaborationInfoB = base.CreateCollaborationInfo();
                collaborationInfoB.ConversationId = "not-equal";
                // Act
                bool isEqual = collaborationInfoA.Equals(collaborationInfoB);
                // Assert
                Assert.False(isEqual);
            }

            [Fact]
            public void ThenTwoCollaborationInfosAreNotEqualForAgreementRef()
            {
                // Arrange
                CollaborationInfo collaborationInfoA = base.CreateCollaborationInfo();
                CollaborationInfo collaborationInfoB = base.CreateCollaborationInfo();
                collaborationInfoB.AgreementRef = new Agreement {Value = "not-equal"};
                // Act
                bool isEqual = collaborationInfoA.Equals(collaborationInfoB);
                // Assert
                Assert.False(isEqual);
            }

            [Fact]
            public void ThenTwoCollaborationInfosAreNotEqualForService()
            {
                // Arrange
                CollaborationInfo collaborationInfoA = base.CreateCollaborationInfo();
                CollaborationInfo collaborationInfoB = base.CreateCollaborationInfo();
                collaborationInfoB.Service = new Service {Value = "not-equal"};
                // Act
                bool isEqual = collaborationInfoA.Equals(collaborationInfoB);
                // Assert
                Assert.False(isEqual);
            }
        }

        public class GivenInvalidArguments : GivenCollaborationInfoFacts
        {
            [Fact]
            public void ThenTwoCollaborationInfosAreNotEqualForNull()
            {
                // Arrange
                CollaborationInfo collaborationInfoA = base.CreateCollaborationInfo();
                CollaborationInfo collaborationInfoB = null;
                // Act
                bool isEqual = collaborationInfoA.Equals(collaborationInfoB);
                // Assert
                Assert.False(isEqual);
            }
        }
    }
}