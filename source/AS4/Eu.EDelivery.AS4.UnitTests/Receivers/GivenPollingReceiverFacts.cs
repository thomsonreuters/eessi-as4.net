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
            this._validTemplate = new StubValidPollingTemplate();
            this._invalidTemplate = new StubInvalidPollingTemplate();
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
                this._cancellationTokenSource.Cancel();

                return null;
            }

            [Fact]
            public void ThenStartPollingSucceeds()
            {
                // Arrange
                base._cancellationTokenSource = new CancellationTokenSource();

                // Act
                base._validTemplate.Start(AssertMessageReceived, base._cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Testing if the receiver fails
        /// </summary>
        public class GivenPollingReceiverFails : GivenPollingReceiverFacts
        {
            private Task<InternalMessage> AssertMessageReceived(string message, CancellationToken cancellationToken)
            {
                return null;
            }

            [Fact]
            public void ThenTemplateFailsWithZeroPollingInterval()
            {
                // Arrange
                base._cancellationTokenSource = new CancellationTokenSource();

                // Act
                Assert.Throws<ApplicationException>(
                    () => base._invalidTemplate.Start(AssertMessageReceived, base._cancellationTokenSource.Token));
            }
        }
    }
}