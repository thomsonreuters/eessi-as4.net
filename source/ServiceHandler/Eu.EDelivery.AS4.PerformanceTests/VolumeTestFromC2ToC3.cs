using System;
using System.Diagnostics;
using Xunit;
using Xunit.Sdk;
using static Eu.EDelivery.AS4.VolumeTests.Properties.Resources;

namespace Eu.EDelivery.AS4.VolumeTests
{
    /// <summary>
    /// 1. C2 (product A) to C3 (product B) oneWay signed and encrypted with increasing number of messages of a 10KB payload.
    /// </summary>
    public class VolumeTestFromC2ToC3 : VolumeTestBridge
    {
        [Fact]
        public void TestSendingHundredMessages()
        {
            // Arrange
            const int messageCount = 100;

            // Act
            Corner2.PlaceMessages(messageCount, SIMPLE_ONEWAY_TO_C3);

            // Assert
            PollingTill(messageCount, Corner3, () => AssertMessages(messageCount));
        }
        
        [Fact]
        public void MeasureSendingHundredMessages()
        {
            // Arrange
            const int messageCount = 100;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Act
            Corner2.PlaceMessages(messageCount, SIMPLE_ONEWAY_TO_C3);

            bool allMessagesDelivered = Corner3.ExecuteWhenNumberOfMessagesAreDelivered(messageCount, () => { sw.Stop(); }, TimeSpan.FromSeconds(90), "*.xml");

            Assert.True(allMessagesDelivered);

            Console.WriteLine($"Processing {messageCount} messages took {sw.Elapsed.TotalSeconds} seconds");
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
    }
}
