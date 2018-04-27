using System;
using Eu.EDelivery.AS4.PerformanceTests.Fixture;
using Xunit;
using static Eu.EDelivery.AS4.PerformanceTests.Properties.Resources;

namespace Eu.EDelivery.AS4.PerformanceTests.LargeMessages
{
    /// <summary>
    /// 1. C2 (product A) to C3 (product B) oneWay signed and encrypted with increasing payload size - triggered by M-K on C1.
    /// </summary>
    public class LargeMessagesTestFromC2ToC3 : PerformanceTestBridge
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LargeMessagesTestFromC2ToC3"/> class.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        public LargeMessagesTestFromC2ToC3(CornersFixture fixture) : base(fixture) {}

        [Theory]
        [InlineData(64, Size.MB)]
        [InlineData(128, Size.MB)]
        [InlineData(256, Size.MB)]
        [InlineData(512, Size.MB)]
        [InlineData(1, Size.GB, 50)]
        [InlineData(2, Size.GB, 100)]
        [InlineData(3, Size.GB, 150)]
        public void TestIncreasingPayloadSize(int unit, Size metric, int retryCount = 20)
        {
            Console.WriteLine(@"Start Large Message Performance Test: " + unit + metric);

            // Act
            Corner2.PlaceLargeMessage(unit, metric, SIMPLE_ONEWAY_TO_C3_SIZE);

            // Assert
            PollingTillFirstPayload(Corner3, retryCount, assertion: () => AssertMessages(unit * (int) metric));
        }

        private void AssertMessages(int expectedSize)
        {
            int actualSize = Corner3.FirstDeliveredMessageLength("*.jpg");
            int Floor(int i) => (int) (i * 0.1);

            Assert.Equal(Floor(expectedSize), Floor(actualSize));
        }
    }
}
