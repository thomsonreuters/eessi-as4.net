using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.PMode
{
    /// <summary>
    /// Testing <see cref="PModeActionResolver" />
    /// </summary>
    public class GivenPModeActionResolverFacts
    {
        public class GivenValidArguments : GivenPModeActionResolverFacts
        {
            [Fact]
            public void ThenResolverGetsAction()
            {
                // Arrange
                var pmode = new SendingProcessingMode
                {
                    MessagePackaging = {CollaborationInfo = new AS4.Model.PMode.CollaborationInfo {Action = "action"}}
                };

                // Act
                string action = PModeActionResolver.ResolveAction(pmode);

                // Assert
                Assert.Equal(pmode.MessagePackaging.CollaborationInfo.Action, action);
            }

            [Fact]
            public void ThenResolverGetsDefaultAction()
            {
                // Arrange
                var pmode = new SendingProcessingMode();

                // Act
                string action = PModeActionResolver.ResolveAction(pmode);

                // Assert
                Assert.Equal(Constants.Namespaces.TestAction, action);
            }
        }
    }
}