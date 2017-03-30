using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitActionResolver" />
    /// </summary>
    public class GivenSubmitActionResolverFacts
    {
        public class GivenValidArguments : GivenSubmitActionResolverFacts
        {
            [Fact]
            public void ThenResolverGetsActionFromPMode()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                var pmode = new SendingProcessingMode
                {
                    MessagePackaging = {CollaborationInfo = new CollaborationInfo {Action = "pmode-action"}}
                };
                submitMessage.PMode = pmode;
                var resolver = new SubmitActionResolver();

                // Act
                string action = resolver.Resolve(submitMessage);

                // Assert
                Assert.Equal(pmode.MessagePackaging.CollaborationInfo.Action, action);
            }

            [Fact]
            public void ThenResolverGetsActionFromSubmitMessage()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PMode = new SendingProcessingMode(),
                    Collaboration = {Action = "submit-action"}
                };
                var resolver = new SubmitActionResolver();

                // Act
                string action = resolver.Resolve(submitMessage);

                // Assert
                Assert.Equal(submitMessage.Collaboration.Action, action);
            }

            [Fact]
            public void ThenResolverGetsDefaultAction()
            {
                // Arrange
                var submitMessage = new SubmitMessage {PMode = new SendingProcessingMode()};
                var resolver = new SubmitActionResolver();

                // Act
                string action = resolver.Resolve(submitMessage);

                // Assert
                Assert.Equal(Constants.Namespaces.TestAction, action);
            }
        }

        public class GivenInvalidArguments : GivenSubmitActionResolverFacts
        {
            [Fact]
            public void ThenResolverFailsWhenOverrideIsNotAllowed()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    Collaboration = {Action = "submit-action"},
                    PMode =
                        new SendingProcessingMode
                        {
                            MessagePackaging = {CollaborationInfo = new CollaborationInfo {Action = "pmode-action"}},
                            AllowOverride = false
                        }
                };
                var resolver = new SubmitActionResolver();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => resolver.Resolve(submitMessage));
            }
        }
    }
}