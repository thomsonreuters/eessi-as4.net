using Eu.EDelivery.AS4.Model.Core;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="CollaborationInfo" />
    /// </summary>
    public class GivenCollaborationInfoFacts
    {
        public class GivenValidArguments : GivenCollaborationInfoFacts
        {
            [Fact]
            public void ThenCollaborationInfoHasDefaults()
            {
                // Act
                var collaborationInfo = CollaborationInfo.Default;

                // Assert
                Assert.Equal(Constants.Namespaces.TestAction, collaborationInfo.Action);
                Assert.Equal(CollaborationInfo.DefaultConversationId, collaborationInfo.ConversationId);
            }

            [Fact]
            public void ThenCollaborationInfoHasProperties()
            {
                // Act
                var collaborationInfo = CollaborationInfo.Default;

                // Assert
                Assert.NotNull(collaborationInfo.Service);
                Assert.NotNull(collaborationInfo.AgreementReference);
            }
        }
    }
}