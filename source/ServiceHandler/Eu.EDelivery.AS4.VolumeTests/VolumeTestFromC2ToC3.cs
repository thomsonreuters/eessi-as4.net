using Xunit;
using static Eu.EDelivery.AS4.VolumeTests.Properties.Resources;

namespace Eu.EDelivery.AS4.VolumeTests
{
    /// <summary>
    /// 1. C2 (product A) to C3 (product B) oneWay signed and encrypted with increasing number of messages of a 10KB payload - triggered by M-K on C1
    /// </summary>
    public class VolumeTestFromC2ToC3 : VolumeTestBridge
    {
        [Fact]
        public void TestIncreasingNumberOfMessages()
        {
            // Act
            Corner2.PlaceMessageAtCorner(SIMPLE_ONEWAY_TO_C3);

            // Assert
            AssertOnHunderdFiles("*.jpg");
            AssertOnHunderdFiles("*.xml");
        }

        private void AssertOnHunderdFiles(string searchPattern)
        {
            int fileCount = Corner3.CountDeliveredFiles(searchPattern);

            Assert.Equal(100, fileCount);
        }
    }
}
