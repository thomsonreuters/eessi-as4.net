using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    public class StubReceiver : IReceiver
    {
        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings">Settings to configure the <see cref="IReceiver"/> instance.</param>
        public void Configure(IEnumerable<Setting> settings) {}

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback">Callback for each message that's being received.</param>
        /// <param name="cancellationToken">Cancel the <see cref="IReceiver"/> instance from receiving messages.</param>
        public void StartReceiving(Func<ReceivedMessage, CancellationToken, Task<InternalMessage>> messageCallback, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stop the <see cref="IReceiver"/> instance from receiving.
        /// </summary>
        public void StopReceiving()
        {
            throw new NotImplementedException();
        }
    }
}