using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenDatastoreReceiverFacts"/> class.
        /// </summary>
        public GivenDatastoreReceiverFacts()
        {
            Registry.Instance.MessageBodyStore.Accept(s => true, new StubMessageBodyRetriever(() => Stream.Null));
        }

        [Fact]
        public void CatchesInvalidDatastoreCreation()
        {
            // Arrange
            var receiver = new DatastoreReceiver(
                () => throw new SaboteurException("Sabotage datastore creation"));

            var settings = new DatastoreReceiverSettings("OutMessages", "Operation = ToBeDelivered", "Operation", "Sending");

            receiver.Configure(settings);

            // Act / Assert
            StartReceiverAsTask(receiver, isCalled: false);
        }

        [Fact(Skip = "Multiple update columns are currently not supported")]
        public void ReceiverUpdatesMultipleValues_IfMultipleUpdateSettingsAreSpecified()
        {
            // Arrange
            InsertOutMessageInDatastoreWith(Operation.ToBeDelivered, OutStatus.NotApplicable);

            IReceiver receiver =
                DataStoreReceiverWith(
                    SettingsToPollOnOutMessages(
                        filter: "Operation = ToBeDelivered",
                        updates: new[] { "Operation", "Status" },
                        values: new[] { "Sending", "Sent" }));

            // Act
            StartReceiver(receiver);

            // Assert
            AssertOutMessageIf(
                m => m.Operation.ToEnum<Operation>() == Operation.Sending,
                message =>
                {
                    Assert.Equal(OutStatus.Sent, message.Status.ToEnum<OutStatus>());
                    Assert.Equal(Operation.Sending, message.Operation.ToEnum<Operation>());
                });
        }

        [Fact]
        public void ReceivesOutMessage()
        {
            // Arrange
            Stream expectedStream = Stream.Null;
            const string expectedType = Constants.ContentTypes.Soap;

            ArrangeOutMessageInDatastore(Operation.ToBeDelivered, expectedStream, expectedType);

            var receiver = new DatastoreReceiver(GetDataStoreContext);

            var settings = new DatastoreReceiverSettings("OutMessages", "Operation = \'ToBeDelivered\'", "Operation", "Sending");

            receiver.Configure(settings);

            // Act
            ReceivedMessage actualMessage = StartReceiver(receiver);

            // Assert
            Assert.Equal(expectedStream, actualMessage.UnderlyingStream);
            Assert.Equal(expectedType, actualMessage.ContentType);
        }

        private void ArrangeOutMessageInDatastore(Operation operation, Stream stream, string contentType)
        {
            var stubRetriever = new StubMessageBodyRetriever(() => stream);
            Registry.Instance.MessageBodyStore.Accept(s => s.Contains("test://"), stubRetriever);

            using (DatastoreContext context = GetDataStoreContext())
            {
                var outMessage = new OutMessage("message-id")
                {                    
                    MessageLocation = "test://",
                    ContentType = contentType
                };

                outMessage.SetOperation(operation);

                context.OutMessages.Add(outMessage);

                context.SaveChanges();
            }
        }

        private void InsertOutMessageInDatastoreWith(Operation operation, OutStatus status)
        {
            using (var context = GetDataStoreContext())
            {
                var expectedMessage = new OutMessage("message-id")
                {                    
                    MessageLocation = "ignored location"
                };

                expectedMessage.SetStatus(status);
                expectedMessage.SetOperation(operation);

                context.OutMessages.Add(expectedMessage);
                context.SaveChanges();
            }
        }

        private IReceiver DataStoreReceiverWith(IEnumerable<Setting> settings)
        {
            var receiver = new DatastoreReceiver(GetDataStoreContext);
            ((IReceiver)receiver).Configure(settings);

            return receiver;
        }

        private static IEnumerable<Setting> SettingsToPollOnOutMessages(
            string filter,
            IReadOnlyList<string> updates,
            IReadOnlyList<string> values)
        {
            var settings = new List<Setting>
            {
                new Setting("Table", "OutMessages"),
                new Setting("Filter", filter),
            };

            for (var index = 0; index < updates.Count; index++)
            {
                string update = updates[index];
                var attributes = new XmlAttribute[] { new StubXmlAttribute("field", update) };
                string value = values[index];

                settings.Add(new Setting("Update", value) { Attributes = attributes });
            }

            return settings;
        }

        private static void StartReceiverAsTask(IReceiver receiver, bool isCalled)
        {
            Task.Run(() => StartReceiver(receiver, isCalled));
        }

        private static ReceivedMessage StartReceiver(IReceiver receiver, bool isCalled = true)
        {
            var tokenSource = new CancellationTokenSource();
            var waitHandle = new ManualResetEvent(false);
            var receivedMessage = new ReceivedMessage(Stream.Null);

            receiver.StartReceiving(
                (message, token) =>
                {
                    waitHandle.Set();
                    tokenSource.Cancel();

                    receivedMessage = message;

                    return Task.FromResult((MessagingContext)new EmptyMessagingContext());
                },
                tokenSource.Token);

            Assert.Equal(isCalled, waitHandle.WaitOne(TimeSpan.FromSeconds(5)));

            tokenSource.Cancel();
            receiver.StopReceiving();

            return receivedMessage;
        }

        private void AssertOutMessageIf(Func<OutMessage, bool> where, Action<OutMessage> assertion)
        {
            using (var context = GetDataStoreContext())
            {
                OutMessage actualMessag = context.OutMessages.Where(where).FirstOrDefault();
                assertion(actualMessag);
            }
        }
    }
}