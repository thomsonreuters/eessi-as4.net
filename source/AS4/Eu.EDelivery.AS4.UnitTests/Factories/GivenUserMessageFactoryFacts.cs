using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Factories
{
    /// <summary>
    /// Testing <see cref="UserMessageFactory" />
    /// </summary>
    public class GivenUserMessageFactoryFacts
    {
        public class GivenValidArguments : GivenUserMessageFactoryFacts
        {
            [Fact]
            public void ThenFactoryCreatesDefaultUserMessage()
            {
                // Arrange
                SendingProcessingMode pmode = CreatePopulatedSendingPMode();

                // Act
                UserMessage userMessage = UserMessageFactory.Instance.Create(pmode);

                // Assert
                Assert.NotNull(userMessage.Sender);
                Assert.NotNull(userMessage.Receiver);
                Assert.NotNull(userMessage.CollaborationInfo.AgreementReference);
            }
        }

        protected SendingProcessingMode CreatePopulatedSendingPMode()
        {
            return AS4XmlSerializer.Deserialize<SendingProcessingMode>(Properties.Resources.sendingprocessingmode);
        }
    }
}