using System;
using System.Threading.Tasks;
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
            [Fact]
            public async Task SenderCatchesAndRetrowsAS4Exception_IfDeliverMessage()
            {
                // Arrange
                var sut = new ReliableSender(deliverSender: new SaboteurSender());

                // Act
                SendResult r = await sut.SendAsync(DummyDeliverMessage());

                // Assert
                Assert.Equal(SendResult.FatalFail, r);
            }

            private static DeliverMessageEnvelope DummyDeliverMessage()
            {
                return new DeliverMessageEnvelope(null, null, null);
            }

            [Fact]
            public async Task SenderCatchesAndRethrowsAS4Exception_IfNotifyMesage()
            {
                // Arrange
                var sut = new ReliableSender(notifySender: new SaboteurSender());

                // Act
                SendResult r = await sut.SendAsync(DummyNotifyMessage());

                // Assert
                Assert.Equal(SendResult.FatalFail, r);
            }

            private static NotifyMessageEnvelope DummyNotifyMessage()
            {
                return new NotifyMessageEnvelope(null, default(Status), null, null, null);
            }
        }
    }
}