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
    /// Testing <see cref="ExponentialIntervalReceiver"/>
    /// </summary>
    public class GivenExponentialIntervalReceiverFacts
    {
        private readonly ManualResetEvent _waitHandle;
        private readonly Seriewatch _seriewatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenExponentialIntervalReceiverFacts"/> class.
        /// </summary>
        public GivenExponentialIntervalReceiverFacts()
        {
            _waitHandle = new ManualResetEvent(initialState: false);
            _seriewatch = new Seriewatch();
        }

        [Fact]
        public void StartReceiver()
        {
            // Arrange
            var receiver = new ExponentialIntervalReceiver(StubConfig.Instance);
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
            var actualPMode = AS4XmlSerializer.Deserialize<SendingProcessingMode>(receivedMessage.RequestStream);
            Assert.Equal("01-pmode", actualPMode.Id);

            if (_seriewatch.TrackSerie(maxSerieCount: 3))
            {
                Assert.True(_seriewatch.GetSerie(1) > _seriewatch.GetSerie(0));
                _waitHandle.Set();
            }

            return Task.FromResult(new InternalMessage());
        }
    }
}