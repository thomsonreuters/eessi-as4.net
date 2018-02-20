using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using FsCheck;
using FsCheck.Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    public class GivenCleanUpAgentFacts : GivenDatastoreFacts
    {
        [Property]
        public Property Only_Overdue_Entries_Are_Deleted(int insertion, int retention)
        {
            // Arrange
            string id = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertOutMessage(
                CreateOutMessage(id, DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(insertion))));
            
            // Act
            ExerciseCleaningEntries(retention);

            // Assert
            bool hasEntries = GetOutMessagesFor(id).Any();
            return (hasEntries == insertion > retention)
                .Classify(hasEntries, "OutMessage isn't deleted")
                .Classify(!hasEntries, "OutMessage is deleted");
        }

        [Property]
        public Property Referenced_Reception_Awareness_Entries_Are_Also_Deleted(int insertion, int retention)
        {
            // Arrange
            string id = Guid.NewGuid().ToString();
            GetDataStoreContext.InsertOutMessage(CreateOutMessage(id, DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(insertion))));
            GetDataStoreContext.InsertReceptionAwareness(new ReceptionAwareness{InternalMessageId = id});

            // Act
            ExerciseCleaningEntries(retention);

            // Assert
            bool hasOutMessages = GetOutMessagesFor(id).Any();
            bool hasReferenced = GetReceptionAwarenessFor(id).Any();
            bool hasEntries = hasOutMessages && hasReferenced;

            return (hasEntries == insertion > retention)
                .Classify(hasEntries, "OutMessage and Reference aren't deleted")
                .Classify(!hasEntries, "OutMessage and Reference are deleted");
        }

        private static OutMessage CreateOutMessage(string ebmsMessageId, DateTimeOffset insertionTime)
        {
            return new OutMessage(ebmsMessageId)
            {
                InsertionTime = insertionTime,
                MessageLocation = Registry.Instance.MessageBodyStore.SaveAS4Message(
                    AS4.Common.Config.Instance.OutMessageStoreLocation,
                    AS4Message.Empty)
            };
        }

        private IEnumerable<OutMessage> GetOutMessagesFor(string id)
        {
            using (DatastoreContext ctx = GetDataStoreContext())
            {
                return ctx.OutMessages.Where(m => m.EbmsMessageId.Equals(id)).ToArray();
            }
        }

        private IEnumerable<ReceptionAwareness> GetReceptionAwarenessFor(string refId)
        {
            using (DatastoreContext ctx = GetDataStoreContext())
            {
                return ctx.ReceptionAwareness.Where(r => r.InternalMessageId.Equals(refId)).ToArray();
            }
        }

        [Property]
        public Property Only_Entries_With_Allowed_Operations_Are_Deleted(Operation op)
        {
            // Arrange
            string id = Guid.NewGuid().ToString();
            InMessage m = CreateInMessage(id, insertionTime: DateTimeOffset.UtcNow.AddDays(-2));
            m.SetOperation(op);
            GetDataStoreContext.InsertInMessage(m);

            // Act
            ExerciseCleaningEntries(retention: 1);

            // Assert
            bool hasEntries = GetInMessagesFor(id).Any();
            return (hasEntries == !AllowedOperations.Contains(op))
                .Collect($"InMessage {(hasEntries ? "isn't" : "is")} deleted, with Operation: {op}");
        }

        private static InMessage CreateInMessage(string ebmsMessageId, DateTimeOffset insertionTime)
        {
            return new InMessage(ebmsMessageId)
            {
                InsertionTime = insertionTime,
                MessageLocation = Registry.Instance.MessageBodyStore.SaveAS4Message(
                    AS4.Common.Config.Instance.InMessageStoreLocation,
                    AS4Message.Empty)
            };
        }

        private void ExerciseCleaningEntries(int retention)
        {
            var sut = new CleanUpAgent(GetDataStoreContext, TimeSpan.FromDays(retention).Negate());

            var cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(TimeSpan.FromTicks(1));
            sut.Start(cancellation.Token).GetAwaiter().GetResult();
        }

        private IEnumerable<InMessage> GetInMessagesFor(string id)
        {
            using (DatastoreContext ctx = GetDataStoreContext())
            {
                return ctx.InMessages.Where(m => m.EbmsMessageId.Equals(id)).ToArray();
            }
        }

        private static IEnumerable<Operation> AllowedOperations =>
            new[]
            {
                Operation.Delivered,
                Operation.Forwarded,
                Operation.Notified,
                Operation.Sent,
                Operation.NotApplicable,
                Operation.Undetermined
            };
}
}

