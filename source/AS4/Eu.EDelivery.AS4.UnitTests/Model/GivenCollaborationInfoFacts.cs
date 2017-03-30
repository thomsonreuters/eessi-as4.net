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
            private const string DefaultAction = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/test";

            [Fact]
            public void ThenCollaborationInfoHasDefaults()
            {
                // Act
                var collaborationInfo = new CollaborationInfo();

                // Assert
                Assert.NotNull(collaborationInfo);
                Assert.Equal(DefaultAction, collaborationInfo.Action);
                Assert.Equal("1", collaborationInfo.ConversationId);
            }

            [Fact]
            public void ThenCollaborationInfoHasProperties()
            {
                // Act
                var collaborationInfo = new CollaborationInfo();

                // Assert
                Assert.NotNull(collaborationInfo);
                Assert.NotNull(collaborationInfo.Service);
                Assert.NotNull(collaborationInfo.AgreementReference);
            }
        }
    }
}