using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Eu.EDelivery.AS4.UnitTests.Strategies.Sender;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Testing <see cref="DatastoreReceiver" />
    /// </summary>
    public class GivenDatastoreReceiverFacts : GivenDatastoreFacts
    {
        [Fact]
        public void CatchesInvalidDatastoreCreation()
        {
            // Arrange
            var receiver = new DatastoreReceiver(
                () => { throw new SaboteurException("Sabotage datastore creation"); });

            receiver.Configure(DummySettings());

            // Act / Assert
            StartReceiver(receiver, isCalled: false);
        }

        private static IEnumerable<Setting> DummySettings()
        {
            const string ignored = "ignored";
            return CreateSettings(ignored, ignored, ignored, ignored);
        }

        [Fact]
        public void ReceivesOutMessage()
        {
            // Arrange
            Stream expectedStream = Stream.Null;
            const string expectedType = Constants.ContentTypes.Soap;

            ArrangeOutMessageInDatastore(Operation.ToBeDelivered, expectedStream, expectedType);

            var receiver = new DatastoreReceiver(GetDataStoreContext);
            receiver.Configure(CreateSettings("OutMessages", "Operation", "ToBeDelivered", "Delivering"));

            // Act
            ReceivedMessage actualMessage = StartReceiver(receiver);

            // Assert
            Assert.Equal(expectedStream, actualMessage.RequestStream);
            Assert.Equal(expectedType, actualMessage.ContentType);
        }

        private void ArrangeOutMessageInDatastore(Operation operation, Stream stream, string contentType)
        {
            var stubRetriever = new StubMessageBodyRetriever(() => stream);
            Registry.Instance.MessageBodyStore.Accept(s => s.Contains("test://"), stubRetriever);

            using (DatastoreContext context = GetDataStoreContext())
            {
                context.OutMessages.Add(
                    new OutMessage
                    {
                        EbmsMessageId = "message-id",
                        Operation = operation,
                        MessageLocation = "test://",
                        ContentType = contentType
                    });

                context.SaveChanges();
            }
        }

        private static ReceivedMessage StartReceiver(IReceiver receiver, bool isCalled = true)
        {
            var tokenSource = new CancellationTokenSource();
            var waitHandle = new ManualResetEvent(false);
            ReceivedMessage receivedMessage = null;

            Task.Run(() => receiver.StartReceiving(
                (message, token) =>
                {
                    waitHandle.Set();
                    tokenSource.Cancel();

                    receivedMessage = message;

                    return Task.FromResult(new InternalMessage());
                },
                tokenSource.Token), tokenSource.Token);

            Assert.Equal(isCalled, waitHandle.WaitOne(TimeSpan.FromSeconds(1)));

            tokenSource.Cancel();
            receiver.StopReceiving();

            return receivedMessage;
        }

        private static IEnumerable<Setting> CreateSettings(string table, string field, string value, string update)
        {
            return new[]
            {
                new Setting("Table", table), new Setting("Field", field), new Setting("Value", value),
                new Setting("Update", update), new Setting("PollingInterval", "1000")
            };
        }
    }
}