using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    /// <summary>
    /// <see cref="IDeliverSender"/>, <see cref="INotifySender"/> implementation to 'Spy' on the configuration.
    /// </summary>
    public class SpySender : IDeliverSender, INotifySender
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="SpySender"/> is called for configuration.
        /// </summary>
        public bool IsConfigured { get; private set; }

        /// <summary>
        /// Configure the <see cref="IDeliverSender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(AS4.Model.PMode.Method method)
        {
            IsConfigured = true;
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="deliverMessage"></param>
        public Task SendAsync(DeliverMessageEnvelope deliverMessage)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="notifyMessage"></param>
        public Task SendAsync(NotifyMessageEnvelope notifyMessage)
        {
            throw new System.NotImplementedException();
        }

        [Fact]
        public void SpyOnConfigure()
        {
            // Arrange
            var sut = new SpySender();

            // Act
            sut.Configure(method: null);

            // Assert
            Assert.True(sut.IsConfigured);
        }

        [Fact]
        public async Task Sabotage_Send()
        {
            // Arrange
            var sut = new SpySender();

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(() => sut.SendAsync(deliverMessage: null));
            await Assert.ThrowsAnyAsync<Exception>(() => sut.SendAsync(notifyMessage: null));
        }
    }
}
