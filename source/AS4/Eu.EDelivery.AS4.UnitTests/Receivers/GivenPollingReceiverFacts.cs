using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Testing <see cref="PollingTemplate{TIn,TOut}" />
    /// with a Stubbed Template: <see cref="StubValidPollingTemplate" />
    /// </summary>
    public class GivenPollingReceiverFacts
    {
        private readonly StubInvalidPollingTemplate _invalidTemplate;
        private readonly StubValidPollingTemplate _validTemplate;
        private CancellationTokenSource _cancellationTokenSource;

        public GivenPollingReceiverFacts()
        {
            _validTemplate = new StubValidPollingTemplate();
            _invalidTemplate = new StubInvalidPollingTemplate();
        }

        /// <summary>
        /// Testing if the receiver succeeds
        /// </summary>
        public class GivenPollingReceiverSucceeds : GivenPollingReceiverFacts
        {
            private Task<InternalMessage> AssertMessageReceived(string message, CancellationToken cancellationToken)
            {
                // Assert
                Assert.NotNull(message);
                Assert.Equal("Message", message);
                _cancellationTokenSource.Cancel();

                return null;
            }

            [Fact]
            public void ThenStartPollingSucceeds()
            {
                // Arrange
                _cancellationTokenSource = new CancellationTokenSource();

                // Act
                _validTemplate.Start(AssertMessageReceived, _cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Testing if the receiver fails
        /// </summary>
        public class GivenPollingReceiverFails : GivenPollingReceiverFacts
        {
            [Fact]
            public void ThenTemplateFailsWithZeroPollingInterval()
            {
                // Arrange
                _cancellationTokenSource = new CancellationTokenSource();

                // Act
                Assert.Throws<ApplicationException>(
                    () => _invalidTemplate.Start((message, token) => null, _cancellationTokenSource.Token));
            }
        }
    }
}