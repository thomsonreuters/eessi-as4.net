using System;
using Eu.EDelivery.AS4.Receivers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    public class GivenIntervalRequestFacts
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void CalculateNewInterval(int calculationCount)
        {
            // Arrange
            TimeSpan minInterval = TimeSpan.FromSeconds(1);
            TimeSpan maxInterval = TimeSpan.FromSeconds(5);
            var sut = new StubInterval(minInterval, maxInterval);

            // Act
            CalculateNewInterval(calculationCount, sut);

            // Assert
            TimeSpan actualInterval = sut.CurrentInterval;
            TimeSpan expectedInterval = TimeSpan.FromSeconds(Math.Pow(1.75, calculationCount - 1));
            Assert.Equal(expectedInterval, actualInterval);
        }

        [Fact]
        public void ResetInterval()
        {
            // Arrange
            var sut = new StubInterval(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
            CalculateNewInterval(calculationCount: 2);

            // Act
            sut.ResetInterval();

            // Assert
            Assert.Equal(TimeSpan.Zero, sut.CurrentInterval);
        }

        private static void CalculateNewInterval(int amount, IntervalRequest request)
        {
            if (amount == 0) return;

            request.CalculateNewInterval();
            CalculateNewInterval(amount - 1, request);
        }
    }

    public class StubInterval : IntervalRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubInterval"/> class.
        /// </summary>
        /// <param name="minInterval">The min Interval.</param>
        /// <param name="maxInterval">The max Interval.</param>
        public StubInterval(TimeSpan minInterval, TimeSpan maxInterval) : base(minInterval, maxInterval) {}
    }
}
