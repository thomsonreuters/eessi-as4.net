using System;
using System.Diagnostics;
using System.Threading;

namespace Eu.EDelivery.AS4.PerformanceTests
{
    /// <summary>
    /// Bridge to add an extra abstraction layer for the AS4 Corner creation/destruction.
    /// </summary>
    public class PerformanceTestBridge : IDisposable
    {
        private readonly Stopwatch _stopWatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTestBridge" /> class.
        /// </summary>
        public PerformanceTestBridge()
        {
            Corner2 = Corner.StartNew("c2");
            Corner3 = Corner.StartNew("c3");

            Corner2.CleanupMessages();
            Corner3.CleanupMessages();

            _stopWatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets the facade for the AS4 Corner 2 instance.
        /// </summary>
        protected Corner Corner2 { get; }

        /// <summary>
        /// Gets the facade for the AS4 Corner 3 instance.
        /// </summary>
        protected Corner Corner3 { get; }

        /// <summary>
        /// Start the polling of messages on the delivered directory to assert using the <paramref name="assertAction" /> till the
        /// <paramref name="messageCount" /> is reached.
        /// </summary>
        /// <param name="messageCount">Amount of messages to wait for.</param>
        /// <param name="corner">Corner to use as delivered target.</param>
        /// <param name="assertAction">Assertion of delivered messages.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        protected void PollingTill(int messageCount, Corner corner, Action assertAction)
        {
            const int timeOut = 6;
            var retryCount = 0;
            TimeSpan retryInterval = TimeSpan.FromSeconds(10);

            while (retryCount <= timeOut)
            {
                if (messageCount <= corner.CountDeliveredMessages())
                {
                    assertAction();
                    return;
                }

                retryCount++;
                Thread.Sleep(retryInterval);
            }

            // Assert anyway to let the test fail.
            assertAction();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stopWatch.Stop();
            Console.WriteLine($@"Performance Test took: {_stopWatch.Elapsed:g} to run");

            Corner2.Dispose();
            Corner3.Dispose();
        }   
    }
}