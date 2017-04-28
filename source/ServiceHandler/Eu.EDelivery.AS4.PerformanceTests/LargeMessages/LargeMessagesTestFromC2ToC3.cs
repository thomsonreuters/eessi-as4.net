using System;
using Xunit;
using static Eu.EDelivery.AS4.PerformanceTests.Properties.Resources;

namespace Eu.EDelivery.AS4.PerformanceTests.LargeMessages
{
    /// <summary>
    /// 1. C2 (product A) to C3 (product B) oneWay signed and encrypted with increasing payload size - triggered by M-K on C1.
    /// </summary>
    public class LargeMessagesTestFromC2ToC3 : PerformanceTestBridge
    {
        [Theory]
        [InlineData(64, Size.MB)]
        [InlineData(128, Size.MB)]
        [InlineData(256, Size.MB)]
        [InlineData(512, Size.MB)]
        [InlineData(1, Size.GB)]
        [InlineData(2, Size.GB)]
        public void TestIncreasingPayloadSize(int value, Size metric)
        {
            // Act
            Corner2.PlaceLargeMessage(value, metric, SIMPLE_ONEWAY_TO_C3_SIZE);

            // Assert
            PollingTillFirstPayload(Corner3, () => AssertMessages(value * (int) metric));
        }

        private void AssertMessages(int expectedSize)
        {
            int actualSize = Corner3.FirstDeliveredMessageLength("*.jpg");
            Func<int, int> floor = i => i - (i % 10);

            Assert.Equal(floor(expectedSize), floor(actualSize));
        }
    }
}
