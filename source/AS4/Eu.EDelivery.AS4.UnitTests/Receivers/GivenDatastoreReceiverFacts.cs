using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
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
            Registry.Instance.MessageBodyRetrieverProvider.Accept(s => true, new StubMessageBodyRetriever());
        }

        [Fact]
        public void ReceiverUpdatesMultipleValues_IfMultipleUpdateSettingsAreSpecified()
        {
            // Arrange
            InsertOutMessageInDatastoreWith(Operation.ToBeDelivered, OutStatus.NotApplicable);

            IReceiver receiver =
                DataStoreReceiverWith(
                    SettingsToPollOnOutMessages(
                        filter: "Operation = ToBeDelivered",
                        updates: new[] {"Operation", "Status"},
                        values: new[] {"Sending", "Sent"}));

            // Act
            StartReceiver(receiver);

            // Assert
            AssertOutMessageIf(
                m => m.Operation == Operation.Sending,
                message =>
                {
                    Assert.Equal(Operation.Sending, message.Operation);
                    Assert.Equal(OutStatus.Sent, message.Status);
                });
        }

        private void InsertOutMessageInDatastoreWith(Operation operation, OutStatus status)
        {
            using (var context = new DatastoreContext(Options))
            {
                var expectedMessage = new OutMessage
                {
                    EbmsMessageId = "message-id",
                    MessageLocation = "ignored location",
                    Status = status,
                    Operation = operation
                };

                context.OutMessages.Add(expectedMessage);
                context.SaveChanges();
            }
        }

        private IReceiver DataStoreReceiverWith(IEnumerable<Setting> settings)
        {
            var receiver = new DatastoreReceiver(() => new DatastoreContext(Options));
            receiver.Configure(settings);

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
                new Setting("Field", filter),
            };

            for (var index = 0; index < updates.Count; index++)
            {
                string update = updates[index];
                var attributes = new XmlAttribute[] {new StubXmlAttribute("field", update)};
                string value = values[index];

                settings.Add(new Setting("Update", value) {Attributes = attributes});
            }

            return settings;
        }

        private static void StartReceiver(IReceiver receiver)
        {
            var waitHandle = new ManualResetEvent(initialState: false);
            var cancellationSource = new CancellationTokenSource();

            receiver.StartReceiving(
                (m, c) =>
                {
                    waitHandle.Set();
                    cancellationSource.Cancel();
                    return Task.FromResult(new InternalMessage());
                },
                cancellationSource.Token);

            Assert.True(waitHandle.WaitOne(TimeSpan.FromSeconds(5)));
        }

        private void AssertOutMessageIf(Func<OutMessage, bool> where, Action<OutMessage> assertion)
        {
            using (var context = new DatastoreContext(Options))
            {
                OutMessage actualMessag = context.OutMessages.Where(where).FirstOrDefault();
                assertion(actualMessag);
            }
        }
    }
}