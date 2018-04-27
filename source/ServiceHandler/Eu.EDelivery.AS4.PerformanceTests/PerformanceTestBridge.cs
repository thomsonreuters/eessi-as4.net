using System;
using System.Diagnostics;
using System.Threading;
using Eu.EDelivery.AS4.PerformanceTests.Fixture;
using Xunit;

namespace Eu.EDelivery.AS4.PerformanceTests
{
    /// <summary>
    /// Bridge to add an extra abstraction layer for the AS4 Corner creation/destruction.
    /// </summary>
    [Collection(CornersCollection.CollectionId)]
    public class PerformanceTestBridge : IDisposable
    {
        private readonly Stopwatch _stopWatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTestBridge"/> class.
        /// </summary>
        /// <param name="fixture">The fixture.</param>
        public PerformanceTestBridge(CornersFixture fixture)
        {
            Corner2 = fixture.Corner2;
            Corner3 = fixture.Corner3;

            Corner2.CleanupMessages();
            Corner3.CleanupMessages();

            Corner2.Start();
            Corner3.Start();

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
        /// Start polling for a single message on the delivered directory to assert using the <paramref name="assertion"/>
        /// </summary>
        /// <param name="corner">Corner to use as delivered target.</param>
        /// <param name="retryCount">Amount to retry when polling for payloads</param>
        /// <param name="assertion">Assertion of the delivered message.</param>
        protected void PollingTillFirstPayload(Corner corner, int retryCount, Action assertion)
        {
           PollingForMessages(
               predicate: () => corner.CountDeliveredMessages() == 2, 
               assertion: assertion, 
               range: new PollingRange(retryCount, retrySeconds: 15));
        }

        /// <summary>
        /// Start the polling of messages on the delivered directory to assert using the <paramref name="assertion" /> till the
        /// <paramref name="messageCount" /> is reached.
        /// </summary>
        /// <param name="messageCount">Amount of messages to wait for.</param>
        /// <param name="corner">Corner to use as delivered target.</param>
        /// <param name="assertion">Assertion of delivered messages.</param>
        protected void PollingTillAllMessages(int messageCount, Corner corner, Action assertion)
        {
            PollingForMessages(
                predicate: () => messageCount <= corner.CountDeliveredMessages(), 
                assertion: assertion, 
                range: new PollingRange(retryCount: 10, retrySeconds: 10));
        }

        private static void PollingForMessages(Func<bool> predicate, Action assertion, PollingRange range)
        {
            while (range.InRange)
            {
                if (predicate())
                {
                    assertion();
                    return;
                }

                range.Increase();
                Thread.Sleep(range.RetryInterval);
            }

            // Assert anyway to let the test fail.
            assertion();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stopWatch.Stop();
            Console.WriteLine($@"Performance Test took: {_stopWatch.Elapsed:g} to run");

            Corner2.Stop();
            Corner3.Stop();
        }  
    }

    public class PollingRange
    {
        private readonly int _retryCount;
        private int _currentRetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="PollingRange"/> class.
        /// </summary>
        /// <param name="retryCount">The retry Count.</param>
        /// <param name="retrySeconds">The retry Seconds.</param>
        public PollingRange(int retryCount, int retrySeconds)
        {
            _retryCount = retryCount;
            RetryInterval = TimeSpan.FromSeconds(retrySeconds);
        }

        /// <summary>
        /// Gets the retry interval.
        /// </summary>
        /// <value>
        /// The retry interval.
        /// </value>
        public TimeSpan RetryInterval { get; }

        /// <summary>
        /// Gets a value indicating whether [in range].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in range]; otherwise, <c>false</c>.
        /// </value>
        public bool InRange => _currentRetry <= _retryCount;

        /// <summary>
        /// Increases this instance.
        /// </summary>
        public void Increase() => ++_currentRetry;
    }
}