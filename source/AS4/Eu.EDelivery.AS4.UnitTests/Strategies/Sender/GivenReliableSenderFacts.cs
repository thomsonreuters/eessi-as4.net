using System;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    /// <summary>
    /// Testing <see cref="ReliableSender" />
    /// </summary>
    public class GivenReliableSenderFacts
    {
        public class Configure
        {
            [Fact]
            public void SenderDelegatesConfiguration_IfDeliverSender()
            {
                // Arrange
                var stubSender = new SpySender();
                var sut = new ReliableSender(deliverSender: stubSender);

                // Act
                sut.Configure(method: null);

                // Assert
                Assert.True(stubSender.IsConfigured);
            }

            [Fact]
            public void SenderDelegatesConfiguration_IfNotifySender()
            {
                // Arrange
                var stubSender = new SpySender();
                var sut = new ReliableSender(notifySender: stubSender);

                // Act
                sut.Configure(method: null);

                // Assert
                Assert.True(stubSender.IsConfigured);
            }
        }

        public class Send
        {
            private static DeliverMessageEnvelope DummyDeliverMessage => new DeliverMessageEnvelope(null, null, null);
            private static NotifyMessageEnvelope DummyNotifyMessage => new NotifyMessageEnvelope(null, default(Status), null, null);

            [Fact]
            public void SenderCatchesAndRetrowsAS4Exception_IfDeliverMessage()
            {
                // Arrange
                var sut = new ReliableSender(deliverSender: new SaboteurSender());

                // Act / Assert
                AssertReliableSenderOn(() => sut.Send(DummyDeliverMessage));
            }

            [Fact]
            public void SenderCatchesAndRetrhowsAS4Exception_IfNotifyMesage()
            {
                // Arrange
                var sut = new ReliableSender(notifySender: new SaboteurSender());

                // Act / Assert
                AssertReliableSenderOn(() => sut.Send(DummyNotifyMessage));
            }

            private static void AssertReliableSenderOn(Action actAction)
            {
                var as4Exception = Assert.Throws<AS4Exception>(actAction);
                Assert.IsType<SaboteurException>(as4Exception.InnerException);
            }
        }
    }
}