using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Testing <see cref="PullRequestReceiver"/>
    /// </summary>
    public class GivenPullRequestReceiverFacts
    {
        public class Configure
        {
            [Fact]
            public void FailsWithMissingIntervalAttributes()
            {
                // Arrange
                var receiver = new PullRequestReceiver(StubConfig.Instance);
                var invalidReceiverSetting = new Setting("01-send", value: string.Empty);

                // Act
                receiver.Configure(new[] {invalidReceiverSetting});

                // Assert
                AssertOnMessageReceived(receiver);
            }

            [Fact]
            public void FailsWithMissingSendingPMode()
            {
                // Arrange
                var receiver = new PullRequestReceiver(StubConfig.Instance);
                var receiverSetting = new Setting("unknown-pmode", string.Empty);

                // Act
                receiver.Configure(new[] {receiverSetting});

                // Assert
                AssertOnMessageReceived(receiver);
            }

            private static void AssertOnMessageReceived(IReceiver receiver)
            {
                var waitHandle = new ManualResetEvent(initialState: false);

                receiver.StartReceiving(
                    (message, token) =>
                    {
                        waitHandle.Set();
                        return Task.FromResult(InternalMessage.Empty);
                    }, CancellationToken.None);

                Assert.False(waitHandle.WaitOne(TimeSpan.FromMilliseconds(500)));
            }
        }

        public class StartReceiving : IDisposable
        {
            private readonly ManualResetEvent _waitHandle;
            private readonly Seriewatch _seriewatch;

            /// <summary>
            /// Initializes a new instance of the <see cref="StartReceiving"/> class.
            /// </summary>
            public StartReceiving()
            {
                _waitHandle = new ManualResetEvent(initialState: false);
                _seriewatch = new Seriewatch();
            }

            [Fact]
            public void StartReceiver()
            {
                // Arrange
                var receiver = new PullRequestReceiver(StubConfig.Instance);
                Setting receiverSetting = CreateMockReceiverSetting();
                receiver.Configure(new[] {receiverSetting});

                // Act
                receiver.StartReceiving(OnMessageReceived, CancellationToken.None);

                // Assert
                Assert.True(_waitHandle.WaitOne(timeout: TimeSpan.FromMinutes(1)));
            }

            private static Setting CreateMockReceiverSetting()
            {
                var minTimeAttribute = new StubXmlAttribute("tmin", "0:00:01");
                var maxTimeAttribute = new StubXmlAttribute("tmax", "0:00:25");

                return new Setting("01-send", string.Empty)
                {
                    Attributes = new XmlAttribute[] {minTimeAttribute, maxTimeAttribute}
                };
            }

            private Task<InternalMessage> OnMessageReceived(
                ReceivedMessage receivedMessage,
                CancellationToken cancellationToken)
            {
                var actualPMode = AS4XmlSerializer.FromStream<SendingProcessingMode>(receivedMessage.RequestStream);
                Assert.Equal("01-pmode", actualPMode.Id);

                if (_seriewatch.TrackSerie(maxSerieCount: 3))
                {
                    Assert.True(_seriewatch.GetSerie(1) > _seriewatch.GetSerie(0));
                    _waitHandle.Set();
                }

                return Task.FromResult(InternalMessage.Empty);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _waitHandle?.Dispose();
            }
        }
    }
}