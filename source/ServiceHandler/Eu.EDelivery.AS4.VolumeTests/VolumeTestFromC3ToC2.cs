using Xunit;
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
            AssertOnFileCount(messageCount, "*.jpg");
            AssertOnFileCount(messageCount, "*.xml");
        }

        private void AssertOnFileCount(int messageCount, string searchPattern)
        {
            int fileCount = Corner2.CountDeliveredFiles(searchPattern);

            Assert.Equal(messageCount, fileCount);
        }
    }
}