using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Interface which holds the Signature of Receivers
    /// </summary>
    public interface IReceiver
    {
        /// <summary>
        /// Configure the receiver with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        void Configure(IDictionary<string, string> properties);

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        void StartReceiving(
            Func<ReceivedMessage, CancellationToken, Task<InternalMessage>> messageCallback,
            CancellationToken cancellationToken);

        void StopReceiving();
    }
}