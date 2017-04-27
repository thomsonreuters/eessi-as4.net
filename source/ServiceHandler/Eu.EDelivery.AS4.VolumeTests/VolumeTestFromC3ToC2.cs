using Xunit;
using Xunit.Sdk;
using static Eu.EDelivery.AS4.VolumeTests.Properties.Resources;

namespace Eu.EDelivery.AS4.VolumeTests
{
    /// <summary>
    /// 2. C3 (product B) to C2 (product A) oneWay signed and encrypted with increasing number of messages of a 10KB payload - triggered by M-K on C4
    /// </summary>
    public class VolumeTestFromC3ToC2 : VolumeTestBridge
    {
        [Fact]
        public void TestIncreasingNumberOfMessages()
        {
            // Arrange
            const int messageCount = 100;

            // Act
            Corner3.PlaceMessages(messageCount, SIMPLE_ONEWAY_TO_C2);

            // Assert
            PollingTill(messageCount, Corner2, () => AssertMessages(messageCount));
        }

        private void AssertMessages(int messageCount)
        {
            AssertOnFileCount(messageCount, "*.jpg", $"Payloads count expected to be '{messageCount}'");
            AssertOnFileCount(messageCount, "*.xml", $"Deliver Message count expected to be '{messageCount}'");
        }

        private void AssertOnFileCount(int expectedCount, string searchPattern, string userMessage)
        {
            int actualCount = Corner2.CountDeliveredMessages(searchPattern);

            if (expectedCount != actualCount)
            {
                throw new AssertActualExpectedException(expectedCount, actualCount, userMessage);
            }
        }
    }
}