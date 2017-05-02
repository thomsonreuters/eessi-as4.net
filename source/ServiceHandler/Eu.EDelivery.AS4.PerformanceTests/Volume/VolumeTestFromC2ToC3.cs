using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static Eu.EDelivery.AS4.PerformanceTests.Properties.Resources;

namespace Eu.EDelivery.AS4.PerformanceTests.Volume
{
    /// <summary>
    /// 1. C2 (product A) to C3 (product B) oneWay signed and encrypted with increasing number of messages of a 10KB payload.
    /// </summary>
    public class VolumeTestFromC2ToC3 : PerformanceTestBridge
    {
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTestFromC2ToC3"/> class.
        /// </summary>
        /// <param name="output">The console output for the test run.</param>
        public VolumeTestFromC2ToC3(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip = "d")]
        public void TestSendingHundredMessages()
        {
            // Arrange
            const int messageCount = 100;

            // Act
            Corner2.PlaceMessages(messageCount, SIMPLE_ONEWAY_TO_C3);

            // Assert
            PollingTillAllMessages(messageCount, Corner3, () => AssertMessages(messageCount));
        }

        private void AssertMessages(int messageCount)
        {
            AssertOnFileCount(messageCount, "*.jpg", $"Payloads count expected to be '{messageCount}'");
            AssertOnFileCount(messageCount, "*.xml", $"Deliver Message count expected to be '{messageCount}'");
        }

        private void AssertOnFileCount(int expectedCount, string searchPattern, string userMessage)
        {
            int actualCount = Corner3.CountDeliveredMessages(searchPattern);

            if (expectedCount != actualCount)
            {
                throw new AssertActualExpectedException(expectedCount, actualCount, userMessage);
            }
        }

        [Fact(Skip = "d")]
        public void MeasureSendingHundredMessages()
        {
            // Arrange
            const int messageCount = 100;
            TimeSpan maxExecutionTime = TimeSpan.FromSeconds(90);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Act
            Corner2.PlaceMessages(messageCount, SIMPLE_ONEWAY_TO_C3);

            bool allMessagesDelivered =
                Corner3.ExecuteWhenNumberOfMessagesAreDelivered(
                    messageCount,
                    () => { sw.Stop(); },
                    timeout: maxExecutionTime,
                    searchPattern: "*.xml");

            Assert.True(allMessagesDelivered, $"Not all messages were delivered in the specified timeframe ({maxExecutionTime:g})");

            _output.WriteLine($"Processing {messageCount} messages took {sw.Elapsed.TotalSeconds} seconds");
        }       
    }
}
