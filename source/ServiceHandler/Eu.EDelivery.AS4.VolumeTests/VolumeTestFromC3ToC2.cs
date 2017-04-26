using Xunit;
using static Eu.EDelivery.AS4.VolumeTests.Properties.Resources;

namespace Eu.EDelivery.AS4.VolumeTests
{
    /// <summary>
    /// 2. C3 (product B) to C2 (product A) oneWay signed and encrypted with increasing number of messages of a 10KB payload -
    /// triggered by M-K on C4
    /// </summary>
    public class VolumeTestFromC3ToC2 : VolumeTestBridge
    {
        [Fact]
        public void TestIncreasingNumberOfMessages()
        {
            // Act
            Corner3.PlaceMessageAtCorner(SIMPLE_ONEWAY_TO_C2);

            // Assert
            AssertOnHunderdFiles("*.jpg");
            AssertOnHunderdFiles("*.xml");
        }

        private void AssertOnHunderdFiles(string searchPattern)
        {
            int fileCount = Corner2.CountDeliveredFiles(searchPattern);

            Assert.Equal(100, fileCount);
        }
    }
}