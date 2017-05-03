using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Eu.EDelivery.AS4.IntegrationTests.Attributes
{
    /// <summary>
    /// Works just like [Fact] except that failures are retried (by default, 3 times).
    /// </summary>
    [XunitTestCaseDiscoverer("Eu.EDelivery.AS4.IntegrationTests.Attributes.RetryFactDiscoverer", "Eu.EDelivery.AS4.IntegrationTests")]
    public class RetryFactAttribute : FactAttribute
    {
        /// <summary>
        /// Gets or sets the number of retries allowed for a failed test. If unset (or set less than 1), will
        /// default to 3 attempts.
        /// </summary>
        public int MaxRetries { get; set; }
    }

    public class RetryFactDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public RetryFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            var maxRetries = factAttribute.GetNamedArgument<int>("MaxRetries");
            if (maxRetries < 1)
            {
                maxRetries = 3;
            }

            yield return
                new RetryTestCase(
                    _diagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    testMethod,
                    maxRetries);
        }
    }

    [Serializable]
    public class RetryTestCase : XunitTestCase
    {
        private int maxRetries;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", true)]
        public RetryTestCase() {}

        public RetryTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay testMethodDisplay,
            ITestMethod testMethod,
            int maxRetries) : base(diagnosticMessageSink, testMethodDisplay, testMethod, null)
        {
            this.maxRetries = maxRetries;
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            maxRetries = data.GetValue<int>("MaxRetries");
        }

        // This method is called by the xUnit test framework classes to run the test case. We will do the
        // loop here, forwarding on to the implementation in XunitTestCase to do the heavy lifting. We will
        // continue to re-run the test until the aggregator has an error (meaning that some internal error
        // condition happened), or the test runs without failure, or we've hit the maximum number of tries.
        public override async Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            var runCount = 0;

            while (true)
            {
                // This is really the only tricky bit: we need to capture and delay messages (since those will
                // contain run status) until we know we've decided to accept the final result;
                var delayedMessageBus = new DelayedMessageBus(messageBus);

                RunSummary summary = await base.RunAsync(
                                         diagnosticMessageSink,
                                         delayedMessageBus,
                                         constructorArguments,
                                         aggregator,
                                         cancellationTokenSource);
                if (aggregator.HasExceptions || summary.Failed == 0 || ++runCount >= maxRetries)
                {
                    delayedMessageBus.Dispose(); // Sends all the delayed messages
                    return summary;
                }

                diagnosticMessageSink.OnMessage(
                    new DiagnosticMessage(
                        "Execution of '{0}' failed (attempt #{1}), retrying...",
                        DisplayName,
                        runCount));
            }
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("MaxRetries", maxRetries);
        }
    }

    /// <summary>
    /// Used to capture messages to potentially be forwarded later. Messages are forwarded by
    /// disposing of the message bus.
    /// </summary>
    public class DelayedMessageBus : IMessageBus
    {
        private readonly IMessageBus _innerBus;
        private readonly List<IMessageSinkMessage> messages = new List<IMessageSinkMessage>();

        public DelayedMessageBus(IMessageBus innerBus)
        {
            this._innerBus = innerBus;
        }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            lock (messages)
            {
                messages.Add(message);
            }

            // No way to ask the inner bus if they want to cancel without sending them the message, so
            // we just go ahead and continue always.
            return true;
        }

        public void Dispose()
        {
            foreach (IMessageSinkMessage message in messages)
            {
                _innerBus.QueueMessage(message);
            }
        }
    }
}