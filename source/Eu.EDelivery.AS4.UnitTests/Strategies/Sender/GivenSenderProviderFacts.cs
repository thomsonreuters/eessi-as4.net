using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Strategies.Sender;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    /// <summary>
    /// Testing <see cref="DeliverSenderProvider"/>
    /// </summary>
    public class GivenSenderProviderFacts
    {
        [Fact]
        public void GetsAcceptedDeliverSenderFromProvider()
        {
            // Arrange
            var sut = new DeliverSenderProvider();

            // Act / Assert
            TestProviderReturnsExpectedSender(
                acceptSender: sender => sut.Accept(s => true, () => sender),
                getSender: sut.GetDeliverSender);
        }

        [Fact]
        public void GetsAcceptedNotifySenderFromProvider()
        {
            // Arrange
            var sut = new NotifySenderProvider();

            // Act / Assert
            TestProviderReturnsExpectedSender(
                acceptSender: sender => sut.Accept(s => true, () => sender),
                getSender: sut.GetNotifySender);
        }

        [Fact]
        public void ExceptionIsThrownWhenNoSenderIsConfiguredInProvider()
        {
            // Arrange
            var sut = new NotifySenderProvider();

            Assert.Throws<KeyNotFoundException>(() => sut.GetNotifySender("true"));
        }

        private static void TestProviderReturnsExpectedSender<T>(Action<T> acceptSender, Func<string, T> getSender) where T : class
        {
            // Arrange
            const string dummyOperation = "ignored string";
            var expectedSender = new SaboteurSender() as T;
            acceptSender(expectedSender);

            // Act
            T actualSender = getSender(dummyOperation);

            // Assert
            Assert.Equal(expectedSender, actualSender);
        }
    }
}
