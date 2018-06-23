using Eu.EDelivery.AS4.Model.Core;
using Xunit;

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
                var collaborationInfo = new CollaborationInfo();

                // Assert
                Assert.Equal(Constants.Namespaces.TestAction, collaborationInfo.Action);
                Assert.Null(collaborationInfo.ConversationId);
            }

            [Fact]
            public void ThenCollaborationInfoHasProperties()
            {
                // Act
                var collaborationInfo = new CollaborationInfo();

                // Assert
                Assert.NotNull(collaborationInfo.Service);
                Assert.NotNull(collaborationInfo.AgreementReference);
            }
        }
    }
}