using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model.Internal
{
    /// <summary>
    /// Testing <see cref="ReceivedPullMessage"/>
    /// </summary>
    public class GivenReceivedPullMessageFacts
    {
        [Fact]
        public void SendingPModeGetsAssigned()
        {
            // Arrange
            var as4Message = new AS4Message();
            var sendingPMode = new SendingProcessingMode();
            var receivedMessage = new ReceivedPullMessage(sendingPMode);

            // Act
            receivedMessage.AssignProperties(as4Message);

            // Assert
            Assert.Equal(sendingPMode, as4Message.SendingPMode);
        }
    }
}
