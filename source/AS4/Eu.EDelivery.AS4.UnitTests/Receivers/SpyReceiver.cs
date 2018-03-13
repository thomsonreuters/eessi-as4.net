using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    internal class SpyReceiver : IReceiver
    {
        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(initialState: false);
        private MessagingContext _context;

        public MessagingContext Context
        {
            get
            {
                return _context;
            }

            set
            {
                _context = value;
                _waitHandle.Set();
            }
        }

        public bool IsCalled => _waitHandle.WaitOne(TimeSpan.FromSeconds(5));

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings">Settings to configure the <see cref="IReceiver"/> instance.</param>
        public void Configure(IEnumerable<Setting> settings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback">Callback for each message that's being received.</param>
        /// <param name="cancellationToken">Cancel the <see cref="IReceiver"/> instance from receiving messages.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async void StartReceiving(Func<ReceivedMessage, CancellationToken, Task<MessagingContext>> messageCallback, CancellationToken cancellationToken)
        {
            Context = await messageCallback(new ReceivedMessage(Stream.Null), cancellationToken);
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