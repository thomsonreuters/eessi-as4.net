using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Testing <see cref="DatastoreReceiver" />
    /// </summary>
    public class GivenDatastoreReceiverFacts : GivenDatastoreFacts
    {
        private readonly DatastoreReceiver _receiver;
        private SendingProcessingMode _pmode;

        public GivenDatastoreReceiverFacts()
        {
            _receiver = new DatastoreReceiver(
                storeExpression: () => new DatastoreContext(base.Options),
                findExpression: x => x.OutMessages.Where(m => m.Operation == Operation.ToBeSent),
                updatedOperation: Operation.Sending);

            SeedDataStoreWithOutMessage();
        }

        private void SeedDataStoreWithOutMessage()
        {
            // Insert the seed data that is expected by all test methods
            using (var context = new DatastoreContext(base.Options))
            {
                context.OutMessages.Add(CreateStubOutMessage());
                context.SaveChanges();
            }
        }

        private OutMessage CreateStubOutMessage()
        {
            _pmode = new SendingProcessingMode();
            return new OutMessage
            {
                Operation = Operation.ToBeSent,
                PMode = AS4XmlSerializer.ToString(_pmode),
                MessageBody = new byte[0]
            };
        }

        /// <summary>
        /// Testing if the receiver succeeds
        /// </summary>
        public class GivenOutDatastoreReceiverSucceeds : GivenDatastoreReceiverFacts
        {
            [Fact]
            public void ThenConfigureSucceeds()
            {
                // Arrange
               IEnumerable<Setting> properties = CreateDefaultDatastoreReceiverSettings();

                // Act
                _receiver.Configure(properties);

                // Assert
                Assert.Same(properties, properties);
            }

            private static IEnumerable<Setting> CreateDefaultDatastoreReceiverSettings()
            {
                return new[]
                {
                    new Setting("Table", "OutMessages"),
                    new Setting("Field", "Operation"),
                    new Setting("Value", "ToBeSend"),
                    new Setting("Update", "Sending")    
                };
            }

            [Fact]
            public void ThenStartReceivingSucceeds()
            {
                // Arrange
                var source = new CancellationTokenSource();

                // Act
                base._receiver.StartReceiving(
                    (message, token) => AssertOnReceivedMessage(message, source), source.Token);
            }

            private static Task<InternalMessage> AssertOnReceivedMessage(
                ReceivedMessage message, CancellationTokenSource source)
            {
                // Assert
                Assert.NotNull(message);
                Assert.IsType<ReceivedMessageEntityMessage>(message);
                Assert.NotNull(message.RequestStream);

                source.Cancel();
                return Task.FromResult(NullInternalMessage.Instance);
            }
        }
    }
}