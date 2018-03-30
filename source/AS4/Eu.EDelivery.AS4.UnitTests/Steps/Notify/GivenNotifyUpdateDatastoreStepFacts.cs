using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="NotifyUpdateDatastoreStep" />
    /// </summary>
    public class GivenNotifyUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task Then_Update_InMessage_Succeeds()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();

            var inMessage = CreateInMessage(ebmsMessageId);

            GetDataStoreContext.InsertInMessage(inMessage);

            MessagingContext context = CreateMessagingContextForReceivedEntity(inMessage);

            // Act
            await ExerciseUpdateDatastoreEntity<InMessage>(context, ebmsMessageId);

            // Assert
            GetDataStoreContext.AssertInMessage(
                ebmsMessageId,
                m =>
                {
                    Assert.Equal(Operation.Notified, OperationUtils.Parse(m.Operation));
                    Assert.Equal(InStatus.Notified, InStatusUtils.Parse(m.Status));
                });
        }

        private static InMessage CreateInMessage(string id)
        {
            var inMessage = new InMessage(id);
            inMessage.SetOperation(Operation.Notifying);
            inMessage.SetStatus(InStatus.Delivered);

            return inMessage;
        }

        private static MessagingContext CreateMessagingContextForReceivedEntity(Entity entity)
        {
            var receivedMessage = new ReceivedEntityMessage(entity, Stream.Null, string.Empty);

            var context = new MessagingContext(receivedMessage, MessagingContextMode.Unknown);

            return context;
        }

        [Fact]
        public async Task Then_Update_OutMessage_Succeeds()
        {
            // Arrange
            string ebmsMessageId = Guid.NewGuid().ToString();

            var outMessage = CreateOutMessage(ebmsMessageId);

            GetDataStoreContext.InsertOutMessage(outMessage, withReceptionAwareness: false);

            var messagingContext = CreateMessagingContextForReceivedEntity(outMessage);

            // Act
            await ExerciseUpdateDatastoreEntity<OutMessage>(messagingContext, ebmsMessageId);

            // Assert
            GetDataStoreContext.AssertOutMessage(
                ebmsMessageId,
                m =>
                {
                    Assert.Equal(Operation.Notified, OperationUtils.Parse(m.Operation));
                    Assert.Equal(OutStatus.Notified, OutStatusUtils.Parse(m.Status));
                });
        }

        private static OutMessage CreateOutMessage(string sharedId)
        {
            var outMessage = new OutMessage(sharedId);
            outMessage.SetStatus(OutStatus.Ack);
            outMessage.SetOperation(Operation.Notifying);

            return outMessage;
        }

        [Fact]
        public async Task Then_Update_InException_Succeeds()
        {
            // Arrange
            string refToMessageId = Guid.NewGuid().ToString();

            var inException = CreateInException(refToMessageId);

            GetDataStoreContext.InsertInException(inException);

            var context = CreateMessagingContextForReceivedEntity(inException);

            // Act
            await ExerciseUpdateDatastoreEntity<InException>(context, refToMessageId);

            // Assert
            GetDataStoreContext.AssertInException(
                CreateInException(refToMessageId).EbmsRefToMessageId,
                ex => Assert.Equal(Operation.Notified, OperationUtils.Parse(ex.Operation)));
        }

        private static InException CreateInException(string id)
        {
            var exception = new InException(id, errorMessage: "");
            exception.SetOperation(Operation.ToBeNotified);

            return exception;
        }

        [Fact]
        public async Task Then_Update_OutException_Succeeds()
        {
            // Arrange
            string refToMessageId = Guid.NewGuid().ToString();
            OutException outException = CreateOutException(refToMessageId);
            GetDataStoreContext.InsertOutException(outException);

            var context = CreateMessagingContextForReceivedEntity(outException);

            // Act
            await ExerciseUpdateDatastoreEntity<OutException>(context, refToMessageId);

            // Assert
            GetDataStoreContext.AssertOutException(
                outException.EbmsRefToMessageId,
                ex => Assert.Equal(Operation.Notified, OperationUtils.Parse(ex.Operation)));
        }

        private static OutException CreateOutException(string refId)
        {
            var exception = new OutException(refId, errorMessage: "");
            exception.SetOperation(Operation.ToBeNotified);

            return exception;
        }

        private static async Task ExerciseUpdateDatastoreEntity<T>(MessagingContext context, string ebmsMessageId)
        {
            NotifyMessageEnvelope notifyMessage = CreateNotifyMessage(ebmsMessageId, typeof(T));
            context.ModifyContext(notifyMessage);
            // Act
            var sut = new NotifyUpdateDatastoreStep();
            await sut.ExecuteAsync(context);
        }

        private static NotifyMessageEnvelope CreateNotifyMessage(string id, Type type)
        {
            return new NotifyMessageEnvelope(
                messageInfo: new MessageInfo { MessageId = id, RefToMessageId = id },
                statusCode: Status.Delivered,
                notifyMessage: null,
                contentType: string.Empty,
                entityType: type);
        }
    }
}